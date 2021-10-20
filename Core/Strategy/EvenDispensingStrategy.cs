using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Core.Strategy
{
    /// <summary>
    /// Ensures even dispensing of goods
    /// </summary>
    public class EvenDispensingStrategy : IDispensingStrategy
    {
        public EvenDispensingStrategy(ILayout layout)
        {
            _layout = layout;
        }

        public IEnumerable<DispenseCommand> BuildDispensingChain(Dictionary<string, ushort> cart, PoG planogram)
        {
            throw new System.NotImplementedException();
        }

        private readonly ILayout _layout;
    }
}