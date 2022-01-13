using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Single dispensing unit
    /// </summary>
    public interface IDispenser
    {
        event EventHandler<DispenserTestEventArgs> onTest;
        event EventHandler<string> onResponse;

        uint Id { get; }

        Task Test();

        bool Dispense(string address, uint quantity);

        Task<bool> MultiplyDispensing(IDictionary<string, uint> map);

        /// <summary>
        /// True means that the address is available
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        bool Ping(string address);

        uint GetAddressRank(string address);

        void Reset();
    }
}