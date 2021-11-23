using System;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Single dispensing unit
    /// </summary>
    public interface IDispenser
    {
        event EventHandler<DispenserTestEventArgs> onTest;

        uint Id { get; }

        Task Test();

        bool Dispense(string address, uint quantity);

        /// <summary>
        /// True means that the address is available
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        bool Ping(string address);

        uint GetAddressRank(string address);
    }
}