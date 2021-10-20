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

        public CompositeDispenser(IEnumerable<IDispenser> dispensers, IDispensingStrategy strategy, ILayout layout, PoG planogram)
        {
            _dispensers = dispensers;
            _strategy = strategy;
            _layout = layout;
            _planogram = planogram;

            // Check how the layout corresponds with the planogram
            if (planogram.Addresses.Any(x => _layout.GetBelt(x.Address) == null))
                throw new ArgumentException("There is a poor match between the planogram and the layout");
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
            foreach (var machine in _layout.Machines)
            {
                IDispenser dispenser = _dispensers.FirstOrDefault(x => x.Id == machine.Number);
                if (dispenser == null)
                    continue;

                IEnumerable<CompositDispenseAddress> available =
                    dispenser.AreAddressesAvailable(machine.Trays.SelectMany(x => x.Belts)
                    .Select(x => CompositDispenseAddress.Create(machine.Number, x.Address)));

                foreach (var t in machine.Trays)
                    foreach (var b in t.Belts)
                        b.SetActive(available.Any(x => x.Address == b.Address));
            }
        }

        private readonly IEnumerable<IDispenser> _dispensers;
        private readonly IDispensingStrategy _strategy;
        private readonly ILayout _layout;
        private readonly PoG _planogram;
    }
}