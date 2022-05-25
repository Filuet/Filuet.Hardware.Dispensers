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
        event EventHandler<DispenserTestEventArgs> onTest;
        event EventHandler<(bool direction, string message, string data)> onDataMoving;

        uint Id { get; }

        bool IsAvailable { get; }

        Task Test();

        Task Dispense(string address, uint quantity);

        Task MultiplyDispensing(IDictionary<string, uint> map);

        /// <summary>
        /// True means that the address is available
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        IEnumerable<(string, bool)> Ping(params string[] addresses);

        uint GetAddressRank(string address);

        void Reset();

        /// <summary>
        /// Open door or something
        /// </summary>
        void Unlock();
    }
}