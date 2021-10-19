using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.Core
{
    internal class CompositeDispenser : ICompositeDispenser
    {
        public event EventHandler<DispenseEventArgs> OnDispensing;

        public event EventHandler<ProductDispensedEventArgs> OnDispensingFinished;

        public event EventHandler<DispenserTestEventArgs> OnTest;

        public event EventHandler<DispenseFailEventArgs> OnFailed;

        public CompositeDispenser(IEnumerable<IDispenser> dispensers, IDispensingStrategy strategy, PoG planogram)
        {
            _dispensers = dispensers;
            _strategy = strategy;
            _planogram = planogram;
        }

        public void CheckChannel(CompositDispenseAddress address)
        {
            throw new NotImplementedException();
        }

        public void Dispense(params (string productUid, ushort quantity)[] items)
        {
            PingAddresses();

            IEnumerable<DispenseCommand> dispensingChain = _strategy.BuildDispensingChain(items.ToDictionary(x => x.productUid, y => y.quantity), _planogram);

            foreach (DispenseCommand command in dispensingChain)
            {
                IDispenser _dispenser = _dispensers.SingleOrDefault(x => x.Id == command.Address.VendingMachineID);

                if (_dispenser == null)
                    OnFailed?.Invoke(this, new DispenseFailEventArgs { address = command.Address.Address, message = "Unable to detect the machine" });

                if (_dispenser.Dispense(command.Address, command.Quantity))
                    OnDispensing?.Invoke(this, new DispenseEventArgs { address = command.Address.Address });
            }
        }

        private void PingAddresses()
        {
            foreach (var g in _planogram.Addresses.GroupBy(x => x.VendingMachineID))
            {
                IDispenser dispenser = _dispensers.FirstOrDefault(x => x.Id == g.Key);
                IEnumerable<CompositDispenseAddress> addresses = g.AsEnumerable();
                Parallel.ForEach(addresses, x => x.IsAddressAvailable = false);

                IEnumerable<CompositDispenseAddress> available = dispenser.AreAddressesAvailable(addresses.ToList());
                Parallel.ForEach(available, x => x.IsAddressAvailable = true);
            }
        }

        private readonly IEnumerable<IDispenser> _dispensers;
        private readonly IDispensingStrategy _strategy;
        private readonly PoG _planogram;
    }
}