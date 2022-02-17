using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core.Strategy;
using System;
using System.Collections.Generic;
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

        public CompositeDispenser(IEnumerable<IDispenser> dispensers, DispensingChainBuilder chainBuilder, PoG planogram)
        {
            _dispensers = dispensers;
            foreach (IDispenser d in _dispensers)
            {
                d.onTest += (sender, e) => {
                    onTest?.Invoke(this, new CompositeDispenserTestEventArgs { Dispenser = d, Severity = e.Severity, Message = e.Message });
                };
                d.onResponse += (sender, e) => onResponse?.Invoke(sender, e);
                d.onDispensed += (sender, e) => onDispensed?.Invoke(sender, e);
            }

            _chainBuilder = chainBuilder ;
            _planogram = planogram;

            PingRoutes();
        }

        public async Task Test() =>
            await PingRoutes();

        public async Task Dispense(params (string productUid, ushort quantity)[] items)
        {
            List<string> routesToPing = new List<string>();
            foreach (var i in items)
                routesToPing.AddRange(_planogram[i.productUid].Addresses);

            await PingRoutes(routesToPing);

            IEnumerable<DispenseCommand> dispensingChain = _chainBuilder.BuildChain(items, address => {
                PoGRoute route = _planogram.GetRoute(address);
                return route.Dispenser.GetAddressRank(address);
            });

            //foreach (DispenseCommand command in dispensingChain)
            //    await command.Route.Dispenser.Dispense(command.Route.Address, command.Quantity);

            IEnumerable<IDispenser> dispensers = dispensingChain.Select(x => x.Route.Dispenser).Distinct().OrderBy(x => x.Id);

            foreach (var dispenser in dispensers)
                await dispenser.MultiplyDispensing(dispensingChain.Where(x => x.Route.Dispenser == dispenser).ToDictionary(x => x.Route.Address, y => (uint)y.Quantity));
        }

        private async Task PingRoutes(IEnumerable<string> routes = null)
            => await Task.WhenAll(_dispensers.Select(x => x.Test()).ToArray())
                .ContinueWith(x => {
                    foreach (string route in routes ?? _planogram.Addresses)
                    {
                        bool isRouteAvailable = false;
                        IDispenser correspondentDispenser = null;
                        foreach (IDispenser dispenser in _dispensers)
                            if (dispenser.Ping(route))
                            {
                                correspondentDispenser = dispenser;
                                isRouteAvailable = true;
                                break;
                            }

                        _planogram.SetAttributes(route, correspondentDispenser, isRouteAvailable);
                    }
                });

        internal readonly IEnumerable<IDispenser> _dispensers;
        private readonly DispensingChainBuilder _chainBuilder;
        internal readonly PoG _planogram;
    }
}