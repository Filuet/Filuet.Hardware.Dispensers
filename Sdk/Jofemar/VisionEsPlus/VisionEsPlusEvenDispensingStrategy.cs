using Filuet.ASC.Kiosk.OnBoard.Dispensing.Tests.Entities;
using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    /// <summary>
    /// Ensures even dispensing of goods
    /// </summary>
    public class VisionEsPlusEvenDispensingStrategy : IDispensingStrategy
    {
        public VisionEsPlusEvenDispensingStrategy(ILayout layout)
        {
            _layout = layout;
        }

        public IEnumerable<DispenseCommand> BuildDispensingChain(Dictionary<string, ushort> cart, PoG planogram)
        {
            List<(VisionEspBelt, ushort)> result = new List<(VisionEspBelt, ushort)>();

            foreach (var item in cart)
            {
                IEnumerable<IBelt> activeBelts = _layout.GetBelts(planogram[item.Key].Addresses, true);
                planogram.GetRoutes(activeBelts.);

                if (!activeBelts.Any())
                    throw new InvalidOperationException($"Unable to extract {item.Key}: no address"); // Fire the exception for time being (for debug purposes)




                result.Add(((VisionEspBelt)activeBelts.First(), item.Value));
            }

            

            result.Add(DispenseCommand.Create(((VisionEspBelt)activeBelts.First()).Route, item.Value));
            return result.OrderBy(x => x.Address).ThenBy(x => x.Address.Address);
        }

        private readonly ILayout _layout;
    }
}