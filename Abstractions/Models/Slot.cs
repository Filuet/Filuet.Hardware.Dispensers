namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Slot is an address from which you can extract 1 item of product
    /// </summary>
    public class Slot
    {
        static Slot New(string product, string address)
            => new Slot { Product = product, Address = address };

        public string Product { get; set; }

        public string Address { get; set; }

        public override string ToString()
            => $"{Address} {Product}";
    }
}
