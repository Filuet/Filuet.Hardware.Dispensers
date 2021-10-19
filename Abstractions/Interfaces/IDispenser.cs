using Filuet.ASC.Kiosk.OnBoard.Common.Abstractions.Hardware;
using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions
{
    /// <summary>
    /// Single dispensing unit
    /// </summary>
    public interface IDispenser
    {
        event EventHandler<DeviceTestEventArgs> onTest;

        uint Id { get; }

        void Test();

        bool Dispense(DispenseAddress address, uint quantity);

        bool IsAddressAvailable<T>(T address) where T : new();

        /// <summary>
        /// Check addresses availability
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> AreAddressesAvailable<T>(IEnumerable<T> addresses) where T : new();
    }
}