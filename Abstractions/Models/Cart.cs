using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public class Cart
    {
        public IEnumerable<CartItem> Items { get; set; }

        public IEnumerable<string> Products
            => Items.Select(x => x.ProductUid).Distinct();
    }
}
