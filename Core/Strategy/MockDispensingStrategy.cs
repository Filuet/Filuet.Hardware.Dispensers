using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.Core.Strategy
{
    internal class MockDispensingStrategy : IDispensingStrategy
    {
        public MockDispensingStrategy(ILayout layout)
        {
            _layout = layout;
        }

        public IEnumerable<DispenseCommand> BuildDispensingChain(Dictionary<string, ushort> cart, PoG planogram)
        {
            foreach (var item in cart)
            {
                CompositDispenseAddress address = planogram[item.Key].Addresses.FirstOrDefault();

                if (address == null)
                    throw new InvalidOperationException($"Unable to extract {item.Key}: no address");

                yield return DispenseCommand.Create(address, item.Value);
            }
        }

        private readonly ILayout _layout;
    }
}
