using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface IDispensingStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cart">Product UId with quantity</param>
        /// <param name="planogram"></param>
        /// <returns></returns>
        IEnumerable<DispenseCommand> BuildDispensingChain(Dictionary<string, ushort> cart, PoG planogram);
    }
}