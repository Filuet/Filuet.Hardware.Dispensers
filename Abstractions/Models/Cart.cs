using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public class Cart
    {
        public IEnumerable<CartItem> Items { get; set; }

        public IEnumerable<string> Products
            => Items.Select(x => x.ProductUid).Distinct();

        public void RemoveDispensed(IEnumerable<CartItem> items) {
            if (items == null || !items.Any())
                return;

            foreach (var i in Items)
                i.Quantity -= items.FirstOrDefault(x => x.ProductUid == i.ProductUid)?.Quantity ?? 0;

        }
    }
}
