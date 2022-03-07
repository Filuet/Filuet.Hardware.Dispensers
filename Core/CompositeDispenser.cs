﻿using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core.Strategy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("PoC")]

namespace Filuet.Hardware.Dispensers.Core
{
    internal class CompositeDispenser : ICompositeDispenser
    {
        public event EventHandler<string> onResponse;
        public event EventHandler<DispenseEventArgs> onDispensed;
        public event EventHandler<ProductDispensedEventArgs> onDispensingFinished;
        public event EventHandler<CompositeDispenserTestEventArgs> onTest;
        public event EventHandler<DispenseFailEventArgs> onFailed;
        public event EventHandler<PlanogramEventArgs> onPlanogramClarification;

        public CompositeDispenser(IEnumerable<IDispenser> dispensers, DispensingChainBuilder chainBuilder, PoG planogram)
        {
            _dispensers = dispensers;
            foreach (IDispenser d in _dispensers)
            {
                d.onTest += (sender, e) =>
                {
                    onTest?.Invoke(this, new CompositeDispenserTestEventArgs { Dispenser = d, Severity = e.Severity, Message = e.Message });
                };
                d.onResponse += (sender, e) => onResponse?.Invoke(sender, e);
                d.onDispensed += (sender, e) => onDispensed?.Invoke(sender, e);
            }

            _chainBuilder = chainBuilder;
            _planogram = planogram;

            Test();
        }

        public async Task Dispense(params (string productUid, ushort quantity)[] items)
        {
            List<string> routesToPing = new List<string>();
            foreach (var i in items)
                routesToPing.AddRange(_planogram[i.productUid].Addresses);

            try
            {
                await Test(); // update status of dispenser

                IEnumerable<DispenseCommand> dispensingChain = _chainBuilder.BuildChain(items, address =>
                {
                    PoGRoute route = _planogram.GetRoute(address);
                    return route.Dispenser.GetAddressRank(address);
                }, x => PingRoute(x));

                IEnumerable<IDispenser> dispensers = dispensingChain.Select(x => x.Route.Dispenser).Distinct().OrderBy(x => x.Id);

                foreach (var dispenser in dispensers)
                    await dispenser.MultiplyDispensing(dispensingChain.Where(x => x.Route.Dispenser == dispenser).ToDictionary(x => x.Route.Address, y => (uint)y.Quantity));
            }
            catch (InvalidOperationException ex)
            {
                onFailed?.Invoke(this, new DispenseFailEventArgs { message = ex.Message });
            }
        }

        public void Unlock(params uint[] machines)
        {
            Parallel.ForEach(_dispensers, x =>
            {
                if (!machines.Any() || machines.Contains(x.Id))
                    x.Unlock();
            });
        }

        public async Task Test() => await Task.WhenAll(_dispensers.Select(x => x.Test()).ToArray());

        private async Task PingRoutes(IEnumerable<string> routes = null)
            => await Task.WhenAll(_dispensers.Select(x => x.Test()).ToArray())
                .ContinueWith(x =>
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    Parallel.ForEach(_dispensers, d =>
                    {
                        var pingResult = d.Ping(_planogram.Addresses.ToArray()).ToList();
                        foreach (var a in pingResult)
                            if (a.Item2)
                                _planogram.SetAttributes(a.Item1, d, true);
                    });

                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram });
                    sw.Stop();
                    Console.WriteLine($"Diagnostic time: {sw.Elapsed.TotalSeconds} sec");
                });

        private bool PingRoute(string route)
        {
            bool isAvailable = false;

            foreach (var d in _dispensers)
            {
                if (!d.IsAvailable)
                    continue;

                var pingResult = d.Ping(route);
                foreach (var a in pingResult)
                    if (a.Item2)
                    {
                        isAvailable = true;
                        _planogram.SetAttributes(a.Item1, d, true);
                    }
            }

            onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram });

            return isAvailable;
        }

        internal readonly IEnumerable<IDispenser> _dispensers;
        private readonly DispensingChainBuilder _chainBuilder;
        internal readonly PoG _planogram;
    }
}