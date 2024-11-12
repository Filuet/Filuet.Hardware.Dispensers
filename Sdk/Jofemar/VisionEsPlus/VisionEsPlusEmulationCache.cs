using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums;
using Filuet.Infrastructure.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusEmulationCache
    {
        const string DOOR_OPENED = "doorOpened";
        const string REBOOT = "reset";
        const string LIFT_UNLOCKED = "unlock";
        const string DISPENSING = "dispensing";

        private readonly MemoryCacher _cache;
        private Queue<(DispenserStateSeverity, VisionEsPlusResponseCodes, string)> _stateQueue = new Queue<(DispenserStateSeverity, VisionEsPlusResponseCodes, string)>();

        public VisionEsPlusEmulationCache(MemoryCacher cache)
        {
            _cache = cache;
        }


        /// <summary>
        /// Set door opened
        /// </summary>
        public void InvokeOpenDoor() {
            _cache.Set(DOOR_OPENED, DateTime.Now, TimeSpan.FromMinutes(15).TotalMilliseconds);
            
            // Push door closed event to the states after the timeout
            Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(t => {
                _stateQueue.Enqueue(new (DispenserStateSeverity.Normal, VisionEsPlusResponseCodes.Unknown, "the door is closed"));
            });
        }

        public void InvokeReboot()
            => _cache.Set(REBOOT, DateTime.Now, TimeSpan.FromSeconds(10).TotalMilliseconds);

        /// <summary>
        /// unlock the door of the elevator
        /// </summary>
        public void InvokeUnlock()
            => _cache.Set(LIFT_UNLOCKED, DateTime.Now, TimeSpan.FromSeconds(5).TotalMilliseconds);

        public void InvokeDispense()
            => _cache.Set(DISPENSING, DateTime.Now, TimeSpan.FromSeconds(5).TotalMilliseconds);

        public (DispenserStateSeverity, VisionEsPlusResponseCodes, string)? GetState() {
            return _stateQueue.Any() ? _stateQueue.Dequeue() : null;
        }

        public bool IsDoorOpened() {
            DateTime openedAt = _cache.Get<DateTime>(DOOR_OPENED);
            return openedAt != DateTime.MinValue;
        }

        public bool IsRebooting() {
            DateTime rebootSentAt = _cache.Get<DateTime>(REBOOT);
            return rebootSentAt != DateTime.MinValue;
        }

        public bool IsUnlocked() {
            DateTime unlockedAt = _cache.Get<DateTime>(LIFT_UNLOCKED);
            return unlockedAt != DateTime.MinValue;
        }

        public bool IsDispensing() {
            DateTime dispensingInvokedAt = _cache.Get<DateTime>(DISPENSING);
            return dispensingInvokedAt != DateTime.MinValue;
        }

        public void RaiseEmptyBelt(string address)
            => _stateQueue.Enqueue(new(DispenserStateSeverity.Inoperable, VisionEsPlusResponseCodes.EmptyChannel, $"Belt is empty {address}"));

        public void RaiseInvalidAddress(string address)
            => _stateQueue.Enqueue(new(DispenserStateSeverity.Inoperable, VisionEsPlusResponseCodes.InvalidChannelRequested, $"Invalid address {address}"));
    }
}