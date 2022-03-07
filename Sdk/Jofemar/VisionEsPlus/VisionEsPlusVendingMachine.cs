﻿using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusVendingMachine : IDispenser
    {
        public event EventHandler<DispenseEventArgs> onDispensed;
        public event EventHandler<DispenserTestEventArgs> onTest;
        public event EventHandler<string> onResponse;

        public uint Id { get; private set; }

        public VisionEsPlusVendingMachine(uint id, VisionEsPlus machineAdapter)
        {
            _machineAdapter = machineAdapter;
            Id = id;
            _machineAdapter.onStatus += (sender, e) => onResponse?.Invoke(this, e);
        }

        public async Task Test()
        {
            (DispenserStateSeverity severity, VisionEsPlusResponseCodes origin, string message)? testResult = await _machineAdapter.Status();

            if (testResult.HasValue)
            {
                IsAvailable = testResult.Value.severity == DispenserStateSeverity.Normal;
                onTest?.Invoke(this, new DispenserTestEventArgs { Severity = testResult.Value.severity, Message = testResult.Value.message });
            }
        }

        // There is no use to dispense one by one
        public async Task Dispense(string address, uint quantity)
            => await _machineAdapter.MultiplyDispensing(address, quantity);    

        /// <summary>
        /// Be sure that common weight of the products to be dispensed must be less than weight threshold of the elevator
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public async Task MultiplyDispensing(IDictionary<string, uint> map)
            => await _machineAdapter.MultiplyDispensing(map.ToDictionary(x => (Models.EspBeltAddress)x.Key, x => x.Value));

        public IEnumerable<(string, bool)> Ping(params string[] addresses)
        {
            foreach (string address in addresses)
                yield return (address, IsAvailable ? _machineAdapter.IsBeltAvailable(Id, address) : false);
        }

        public uint GetAddressRank(string address)
        {
            string[] mtb = address.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (mtb.Length != 3)
                throw new ArgumentException($"Invalid dispensing address: {address}");

            if (ushort.TryParse(mtb[0], out ushort machine) && ushort.TryParse(mtb[1], out ushort tray) && ushort.TryParse(mtb[2], out ushort belt))
                return (uint)(machine * 10000 + 100 * tray + belt);
            else throw new ArgumentException($"Invalid dispensing address: {address}");
        }

        public void Reset()
        {
            _machineAdapter.Reset();
        }

        public void Unlock()
        {
            _machineAdapter.Unlock();
        }

        public override string ToString() => $"{Id} [{typeof(VisionEsPlus).Name}]. { (IsAvailable ? "Available" : "Unavailable") }";

        public bool IsAvailable { get; private set; }

        private VisionEsPlus _machineAdapter;
    }
}