using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.Core.Strategy
{
    public class DispensingChainBuilder
    {
        public DispensingChainBuilder(PoG planogram) { _planogram = planogram; }

        public IEnumerable<DispenseCommand> BuildChain((string productUid, ushort quantity)[] cart, Func<string, uint> getRankByRoute)
        {
            Dictionary<string /*address*/, ushort /*qty*/> addressesToDispense = new Dictionary<string, ushort>();

            foreach (var item in cart)
            {
                IEnumerable<PoGRoute> routes = _planogram[item.productUid].Routes.Where(x => x.Active && x.Quantity > 0).ToList();
                if (!routes.Any())
                    throw new InvalidOperationException($"Unable to extract {item.productUid}: no address");

                if (routes.Sum(x => x.Quantity) < item.quantity)
                    throw new InvalidOperationException($"Unable to extract {item.productUid}: lack of goods");

                ushort reserved = 0;
                Dictionary<string /*address*/, ushort /*qty*/> slots = routes.Select(x => (x.Address, x.Quantity)).OrderByDescending(x => x.Quantity).ToDictionary(x => x.Address, x => x.Quantity);

                while (true)
                {
                    ushort maxQty = slots.First().Value;

                    foreach (var s in slots.Where(x => x.Value == maxQty))
                    {
                        slots[s.Key]--;
                        reserved++;
                        if (addressesToDispense.ContainsKey(s.Key))
                            addressesToDispense[s.Key]++;
                        else addressesToDispense.Add(s.Key, 1);

                        if (reserved == item.quantity)
                            break;
                    }

                    if (reserved == item.quantity)
                        break;
                }
            }

            if (addressesToDispense.Select(x=>x.Value).Sum(x => x) != cart.Sum(x => x.quantity))
                throw new InvalidOperationException("An error occured while building the chain of dispensing");

            addressesToDispense = addressesToDispense.OrderBy(x => getRankByRoute(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            foreach (var a in addressesToDispense)
                yield return DispenseCommand.Create(_planogram.GetRoute(a.Key), a.Value);
        }

        private readonly PoG _planogram;
    }
}