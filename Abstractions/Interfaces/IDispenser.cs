using System;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Single dispensing unit
    /// </summary>
    public interface IDispenser
    {
        event EventHandler<DispenserTestEventArgs> onTest;

        uint Id { get; }

        void Test();

        bool Dispense(string address, uint quantity);

        bool IsAddressAvailable(string address);

        /// <summary>
        /// Check addresses availability
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> AreAddressesAvailable(IEnumerable<string> addresses);
    }
}