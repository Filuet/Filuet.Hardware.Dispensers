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
        event EventHandler onReset;

        event EventHandler<(bool direction, string message, string data)> onDataMoving;

        string Alias { get; }

        uint Id { get; }

        bool IsAvailable { get; }

        Task Test();

        Task Dispense(string address, uint quantity);

        Task MultiplyDispensing(IDictionary<string, uint> map);

        /// <summary>
        /// True means that the address is available
        /// </summary>
        /// <param name="addresses"></param>
        /// <remarks>if isActive is null than the address doesn't belong to the dispenser</remarks>
        /// <returns></returns>
        IEnumerable<(string address, bool? isActive)> Ping(params string[] addresses);

        uint GetAddressRank(string address);

        Task Reset();

        /// <summary>
        /// Open door or something
        /// </summary>
        void Unlock();
    }
}