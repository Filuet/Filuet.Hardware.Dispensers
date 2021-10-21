using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusVendingMachine : IDispenser
    {
        public event EventHandler<DispenserTestEventArgs> onTest;
        private VisionEsPlus _machineAdapter;

        public uint Id { get; private set; }

        public VisionEsPlusVendingMachine(uint id, VisionEsPlus machineAdapter)
        {
            _machineAdapter = machineAdapter;
            Id = id;
        }

        public void Test()
        {
            (DispenserStateSeverity severity, string message) testResult = _machineAdapter.Status();
            onTest?.Invoke(this, new DispenserTestEventArgs { Severity = testResult.severity, Message = testResult.message });
        }

        public bool Dispense(DispensingAddress address, uint quantity)   
        {
            var t = _machineAdapter.Status(false);
            bool result = _machineAdapter.DispenseProduct(address, quantity);
            var t1 = _machineAdapter.Status(false);

            return result;
        }

        public bool IsAddressAvailable<T>(T address) where T : new()
            => _machineAdapter.IsBeltAvailable((address as DispensingAddress));

        public IEnumerable<DispensingRoute> AreAddressesAvailable1(IEnumerable<DispensingRoute> addresses)
        {
            foreach (var a in addresses)
                if (IsAddressAvailable(a))
                    yield return a;
        }

        public IEnumerable<T> AreAddressesAvailable<T>(IEnumerable<T> addresses) where T : new()
        {
            foreach (var a in addresses)
                if (IsAddressAvailable(a))
                    yield return a;
        }
    }
}