using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core.Strategy;
using System;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Core
{
    internal class CompositeDispenser : ICompositeDispenser
    {
        public event EventHandler<DispenseEventArgs> OnDispensing;
        public event EventHandler<ProductDispensedEventArgs> OnDispensingFinished;
        public event EventHandler<DispenserTestEventArgs> OnTest;
        public event EventHandler<DispenseFailEventArgs> OnFailed;

        public CompositeDispenser(IEnumerable<IDispenser> dispensers, DispensingChainBuilder chainBuilder, PoG planogram)
        {
            _dispensers = dispensers;
            _chainBuilder = chainBuilder;
            _planogram = planogram;
        }

        public void CheckChannel(string route)
        {
            throw new NotImplementedException();
        }

        public void Dispense(params (string productUid, ushort quantity)[] items)
        {
            PingAddresses();

            IEnumerable<DispenseCommand> dispensingChain = _chainBuilder.BuildChain(items, address => {
                PoGRoute route = _planogram.GetRoute(address);
                return route.Dispenser.GetAddressRank(address);
            });

            foreach (DispenseCommand command in dispensingChain)
            {
                if (command.Route.Dispenser.Dispense(command.Route.Address, command.Quantity))
                    OnDispensing?.Invoke(this, new DispenseEventArgs { address = command.Route.Address });
            }
        }

        private void PingAddresses()
        {
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
        }

        private readonly IEnumerable<IDispenser> _dispensers;
        private readonly DispensingChainBuilder _chainBuilder;
        private readonly PoG _planogram;
    }
}