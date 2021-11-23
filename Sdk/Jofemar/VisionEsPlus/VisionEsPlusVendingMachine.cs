﻿using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using System;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusVendingMachine : IDispenser
    {
        public event EventHandler<DispenserTestEventArgs> onTest;
        private VisionEsPlus _machineAdapter;

        public VisionEsPlusVendingMachine(uint id, VisionEsPlus machineAdapter)
        {
            _machineAdapter = machineAdapter;
            Id = id;
        }

        public async Task Test()
        {
            (DispenserStateSeverity severity, string message) testResult = await _machineAdapter.Status();
            _isAvailable = testResult.severity == DispenserStateSeverity.Normal;
            onTest?.Invoke(this, new DispenserTestEventArgs { Severity = testResult.severity, Message = testResult.message });
        }

        public bool Dispense(string address, uint quantity)
        {
            var t = _machineAdapter.Status();
            bool result = _machineAdapter.DispenseProduct(address, quantity);
            var t1 = _machineAdapter.Status();

            return result;
        }

        public bool Ping(string address)
            => !_isAvailable ? false : _machineAdapter.IsBeltAvailable(address);

        public uint GetAddressRank(string address)
        {
            string[] mtb = address.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (mtb.Length != 3)
                throw new ArgumentException($"Invalid dispensing address: {address}");

            if (ushort.TryParse(mtb[0], out ushort machine) && ushort.TryParse(mtb[1], out ushort tray) && ushort.TryParse(mtb[2], out ushort belt))
                return (uint)(machine * 10000 + 100 * tray + belt);
            else throw new ArgumentException($"Invalid dispensing address: {address}");
        }

        public override string ToString() => $"{Id} [{typeof(VisionEsPlus).Name}]";
        public uint Id { get; private set; }

        public bool _isAvailable = false;
    }
}