using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public class Cart
    {
        public int this[string Product]
            => Items.First(x => x.Sku == Product).Quantity;

        public IEnumerable<CartItem> Items => _items;
        private List<CartItem> _items { get; set; }

        public Cart(IEnumerable<CartItem> items) {
            _items = items.ToList();
        }

        public IEnumerable<string> Products
            => Items.Select(x => x.Sku).Distinct();

        public void RemoveDispensed(string product) {
            CartItem item = _items.FirstOrDefault(x => x.Sku == product);
            if (item == null)
                return;

            item.Quantity = item.Quantity - 1;
            _items = _items.Where(x => x.Quantity > 0).ToList();
        }
    }
}