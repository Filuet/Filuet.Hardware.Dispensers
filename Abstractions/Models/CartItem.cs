namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public class CartItem
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }

        public void Decrease() {
            Quantity = Quantity - 1;
        }

        public override string ToString()
            => $"{Sku}x{Quantity}";
    }
}
