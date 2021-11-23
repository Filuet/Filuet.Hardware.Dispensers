using Filuet.Hardware.Dispensers.Abstractions.Models;

namespace PoC.Models
{
    public class ItemToDispense
    {
        public PoGProduct Product { get; set; }
        public ushort Qty { get; set; }

        public override string ToString() => $"{Product?.ProductUid}x{Qty}";
    }
}
