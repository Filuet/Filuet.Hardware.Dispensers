namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public class CartItem
    {
        public string ProductUid { get; set; }
        public int Quantity { get; set; }

        public void Decrease() {
            Quantity = Quantity - 1;
        }
    }
}
