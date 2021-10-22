using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
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

        public bool Dispense(string address, uint quantity)   
        {
            var t = _machineAdapter.Status(false);
            bool result = _machineAdapter.DispenseProduct(address, quantity);
            var t1 = _machineAdapter.Status(false);

            return result;
        }

        public bool IsAddressAvailable(string address)
            => _machineAdapter.IsBeltAvailable(address);

        public IEnumerable<string> AreAddressesAvailable(IEnumerable<string> addresses)
        {
            foreach (var a in addresses)
                if (IsAddressAvailable(a))
                    yield return a;
        }
    }
}