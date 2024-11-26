using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Single dispensing unit
    /// </summary>
    public interface IDispenser
    {
        /// <summary>
        /// Fires when the customer forgets to pick up the products
        /// </summary>
        event EventHandler<AddressEventArgs> onAbandonment;
        /// <summary>
        /// Fires during dispensing activity
        /// </summary>
        event EventHandler<DispensingFailedEventArgs> onAddressUnavailable;
        /// <summary>
        /// Similar to onDispensingFailed but raises only during belt check
        /// </summary>
        event EventHandler<AddressEventArgs> onAddressInactive;
        event EventHandler<AddressEventArgs> onDispensing;
        event EventHandler<AddressEventArgs> onDispensed;
        event EventHandler<IEnumerable<AddressEventArgs>> onWaitingProductsToBeRemoved;
        event EventHandler<DispenserTestEventArgs> onTest;
        event EventHandler<ResetEventArgs> onReset;

        event EventHandler<(bool direction, string message, string data)> onDataMoving;

        int Id { get; }
        bool IsAvailable { get; }

        Task TestAsync();
        Task<Cart> DispenseAsync(Cart cart);
        /// <summary>
        /// True means that the address is available
        /// </summary>
        /// <param name="addresses"></param>
        /// <remarks>if isActive is null than the address doesn't belong to the dispenser</remarks>
        /// <returns></returns>
        IEnumerable<(string address, bool? isActive)> Ping(params string[] addresses);
        Task ActivateAsync(params string[] addresses);
        Task Reset();
        /// <summary>
        /// Open door or something
        /// </summary>
        void Unlock();
    }
}