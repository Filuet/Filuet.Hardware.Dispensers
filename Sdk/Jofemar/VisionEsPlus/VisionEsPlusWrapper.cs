using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusWrapper : IDispenser, ILightEmitter
    {
        public event EventHandler<DispenseEventArgs> onDispensing;
        public event EventHandler<DispenseEventArgs> onDispensed;
        /// <summary>
        /// Fires when the customer forgets to pick up the products
        /// </summary>
        public event EventHandler<DispenseEventArgs> onAbandonment;
        public event EventHandler<DispenserTestEventArgs> onTest;
        public event EventHandler<ResetEventArgs> onReset;
        public event EventHandler<(bool direction, string message, string data)> onDataMoving;
        public event EventHandler<LightEmitterEventArgs> onLightsChanged;
        public event EventHandler<IEnumerable<DispenseEventArgs>> onWaitingProductsToBeRemoved;
        public event EventHandler<FailedToDispenseEventArgs> onFailedToDispense;
        public event EventHandler<DispenseFailedEventArgs> onAddressUnavailable;

        public int Id => _machineAdapter.Id;

        public bool IsAvailable { get; private set; }

        public VisionEsPlusWrapper(VisionEsPlus machineAdapter) {
            _machineAdapter = machineAdapter;
            _machineAdapter.onDataMoving += (sender, e) => onDataMoving?.Invoke(this, e);
            _machineAdapter.onLightsChanged += (sender, e) => onLightsChanged?.Invoke(this, LightEmitterEventArgs.Create(_machineAdapter.Id, e));
            _machineAdapter.onDispensing += (sender, e) => onDispensing?.Invoke(this, e);
            _machineAdapter.onDispensed += (sender, e) => onDispensed?.Invoke(this, e);
            _machineAdapter.onAbandonment += (sender, e) => onAbandonment?.Invoke(this, e);
            _machineAdapter.onWaitingProductsToBeRemoved += (sender, e) => onWaitingProductsToBeRemoved?.Invoke(this, e);
            _machineAdapter.onFailedToDispense += (sender, e) => onFailedToDispense?.Invoke(this, e);
            _machineAdapter.onAddressUnavailable += (sender, e) => onAddressUnavailable?.Invoke(this, e);
        }

        public async Task TestAsync() {
            (DispenserStateSeverity severity, VisionEsPlusResponseCodes origin, string message)? testResult = await _machineAdapter.StatusAsync();

            if (testResult.HasValue) {
                IsAvailable = testResult.Value.severity == DispenserStateSeverity.Normal;
                onTest?.Invoke(this, new DispenserTestEventArgs { Severity = testResult.Value.severity, Message = testResult.Value.message });
            }
        }

        /// <summary>
        /// Be sure that common weight of the products to be dispensed must be less than weight threshold of the elevator
        /// </summary>
        /// <param name="cart"></param>
        /// <returns>Extracted items</returns>
        public async Task<Cart> DispenseAsync(Cart cart)
            => await _machineAdapter.DispenseAsync(cart);

        public IEnumerable<(string address, bool? isActive)> Ping(params string[] addresses) {
            foreach (string address in addresses) {
                Thread.Sleep(100);
                yield return (address, IsAvailable ? _machineAdapter.IsBeltActive(address) : false);
            }
        }

        public async Task ActivateAsync(params string[] addresses) {
            foreach (string address in addresses) {
                Thread.Sleep(100);
                await _machineAdapter.ActivateBeltAsync(address);
            }
        }

        public async Task Reset() {
            _machineAdapter.Reset();
            (DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)? state = null;

            await Task.Run(async () => {
                while (state == null || state.Value.state == DispenserStateSeverity.Inoperable) {
                    Thread.Sleep(3000);
                    state = await _machineAdapter.StatusAsync();
                }
                onReset?.Invoke(this, new ResetEventArgs { MachineId = _machineAdapter.Id });
            });
        }

        public void Unlock()
            => _machineAdapter.Unlock();

        public void LightOn()
            => _machineAdapter.ChangeLight(true);

        public void LightOff()
            => _machineAdapter.ChangeLight(false);

        public override string ToString()
            => $"Machine {_machineAdapter.Id} [{typeof(VisionEsPlus).Name}]. {(IsAvailable ? "Available" : "Unavailable")}";

        private VisionEsPlus _machineAdapter;
    }
}