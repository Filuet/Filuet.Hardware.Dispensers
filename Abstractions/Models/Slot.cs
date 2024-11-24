namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Slot is an address from which you can extract 1 item of product
    /// </summary>
    public class Slot
    {
        static Slot New(string sku, string address)
            => new Slot { Sku = sku, Address = address };

        public string Sku { get; set; }

        public string Address { get; set; }

        public override string ToString()
            => $"{Address} {Sku}";
    }
}
