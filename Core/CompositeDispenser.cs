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
        public event EventHandler<DispenseEventArgs> onDispensing;
        public event EventHandler<ProductDispensedEventArgs> onDispensingFinished;
        public event EventHandler<CompositeDispenserTestEventArgs> onTest;
        public event EventHandler<DispenseFailEventArgs> onFailed;

        public CompositeDispenser(IEnumerable<IDispenser> dispensers, DispensingChainBuilder chainBuilder, PoG planogram)
        {
            _dispensers = dispensers;
            foreach (IDispenser d in _dispensers)
            {
                d.onTest += (sender, e) => onTest?.Invoke(this, new CompositeDispenserTestEventArgs { Dispenser = d, Severity = e.Severity, Message = e.Message });
                d.onResponse += (sender, e) => onResponse?.Invoke(sender, e);
            }

            _chainBuilder = chainBuilder ;
            _planogram = planogram;

            PingAddresses();
        }

        public async Task Test() =>
            await PingAddresses();

        public async Task Dispense(params (string productUid, ushort quantity)[] items)
        {
            await PingAddresses();

            IEnumerable<DispenseCommand> dispensingChain = _chainBuilder.BuildChain(items, address => {
                PoGRoute route = _planogram.GetRoute(address);
                return route.Dispenser.GetAddressRank(address);
            });

            foreach (DispenseCommand command in dispensingChain)
            {
                if (command.Route.Dispenser.Dispense(command.Route.Address, command.Quantity))
                    onDispensing?.Invoke(this, new DispenseEventArgs { address = command.Route.Address });
            }
        }

        private async Task PingAddresses()
            => await Task.WhenAll(_dispensers.Select(x => x.Test()).ToArray())
                .ContinueWith(x => {
                    foreach (string route in _planogram.Addresses)
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