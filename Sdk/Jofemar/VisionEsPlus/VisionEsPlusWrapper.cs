using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusWrapper : IDispenser, ILightEmitter
    {
        public event EventHandler<DispenseEventArgs> onDispensing;
        public event EventHandler<DispenseEventArgs> onDispensed;
        /// <summary>
        /// Fires when the customer forget to pick up the products
        /// </summary>
        public event EventHandler<DispenseEventArgs> onAbandonment;
        public event EventHandler<DispenserTestEventArgs> onTest;
        public event EventHandler onReset;
        public event EventHandler<(bool direction, string message, string data)> onDataMoving;
        public event EventHandler<LightEmitterEventArgs> onLightsChanged;
        public event EventHandler<IEnumerable<DispenseEventArgs>> onWaitingProductsToBeRemoved;
        public event EventHandler<FailedToDispenseEventArgs> onFailedToDispense;
        public event EventHandler<DispenseFailEventArgs> onAddressUnavailable;

        public VisionEsPlusWrapper(uint id, VisionEsPlus machineAdapter)
        {
            _machineAdapter = machineAdapter;
            Id = id;
            _machineAdapter.onDataMoving += (sender, e) => onDataMoving?.Invoke(this, e);
            _machineAdapter.onLightsChanged += (sender, e) => onLightsChanged?.Invoke(this, LightEmitterEventArgs.Create(Id, Alias, e));
            _machineAdapter.onDispensing += (sender, e) => onDispensing?.Invoke(this, e);
            _machineAdapter.onDispensed += (sender, e) => onDispensed?.Invoke(this, e);
            _machineAdapter.onAbandonment += (sender, e) => onAbandonment?.Invoke(this, e);
            _machineAdapter.onWaitingProductsToBeRemoved += (sender, e) => onWaitingProductsToBeRemoved?.Invoke(this, e);
            _machineAdapter.onFailedToDispense += (sender, e) => onFailedToDispense?.Invoke(this, e);
            _machineAdapter.onAddressUnavailable += (sender, e) => onAddressUnavailable?.Invoke(this, e);
        }

        public async Task TestAsync()
        {
            (DispenserStateSeverity severity, VisionEsPlusResponseCodes origin, string message)? testResult = await _machineAdapter.StatusAsync();

            if (testResult.HasValue)
            {
                IsAvailable = testResult.Value.severity == DispenserStateSeverity.Normal;
                onTest?.Invoke(this, new DispenserTestEventArgs { Severity = testResult.Value.severity, Message = testResult.Value.message });
            }
        }

        /// <summary>
        /// Be sure that common weight of the products to be dispensed must be less than weight threshold of the elevator
        /// </summary>
        /// <param name="map"></param>
        /// <returns>Extracted items</returns>
        public async Task<Cart> DispenseAsync(Cart cart)
            => await _machineAdapter.DispenseAsync(cart);

        public IEnumerable<(string address, bool? isActive)> Ping(params string[] addresses)
        {
            foreach (string address in addresses)
            {
                Thread.Sleep(100);
                yield return (address, IsAvailable ? _machineAdapter.IsBeltActive(Id, address) : false);
            }
        }

        public async Task ActivateAsync(params string[] addresses) {
            foreach (string address in addresses) {
                Thread.Sleep(100);
                await _machineAdapter.ActivateBeltAsync(Id, address);
            }
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

        public async Task Reset()
        {
            _machineAdapter.Reset();
            (DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)? state = null;
            while (state == null || state.Value.state == DispenserStateSeverity.Inoperable)
            {
                Thread.Sleep(3000);
                state = await _machineAdapter.StatusAsync();
            }
            onReset?.Invoke(this, null);
        }

        public void Unlock()
            => _machineAdapter.Unlock();

        public override string ToString()
            => $"{Id} [{typeof(VisionEsPlus).Name}]. {(IsAvailable ? "Available" : "Unavailable")}";

        public void LightOn()
            => _machineAdapter.ChangeLight(true);

        public void LightOff()
            => _machineAdapter.ChangeLight(false);

        public string Alias => _machineAdapter.Alias;

        /// <summary>
        /// Dispenser unique identifier
        /// </summary>
        public uint Id { get; private set; }

        public bool IsAvailable { get; private set; }

        private VisionEsPlus _machineAdapter;
    }
}