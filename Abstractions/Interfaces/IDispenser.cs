﻿using Filuet.Hardware.Dispensers.Abstractions.Models;
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
        event EventHandler<DispenseEventArgs> onDispensing;
        event EventHandler<DispenseEventArgs> onDispensed;
        event EventHandler<DispenseEventArgs> onAbandonment;
        event EventHandler<IEnumerable<DispenseEventArgs>> onWaitingProductsToBeRemoved;
        event EventHandler<DispenserTestEventArgs> onTest;
        event EventHandler<ResetEventArgs> onReset;
        event EventHandler<DispenseFailedEventArgs> onAddressUnavailable;
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