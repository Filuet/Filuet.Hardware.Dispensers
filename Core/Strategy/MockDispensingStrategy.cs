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
                IEnumerable<IBelt> activeBelts = _layout.GetBelts(planogram[item.Key].Addresses, true);
                if (!activeBelts.Any())
                    throw new InvalidOperationException($"Unable to extract {item.Key}: no address");

                // find the best belt to extract

               // yield return DispenseCommand.Create(... , item.Value);
            }

            return null;
        }

        private readonly ILayout _layout;
    }
}
