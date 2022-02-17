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
        event EventHandler<DispenseEventArgs> onDispensed;
        event EventHandler<DispenserTestEventArgs> onTest;
        event EventHandler<string> onResponse;

        uint Id { get; }

        Task Test();

        Task Dispense(string address, uint quantity);

        Task MultiplyDispensing(IDictionary<string, uint> map);

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