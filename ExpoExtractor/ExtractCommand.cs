using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispenser
{
    public class ExtractSlot {
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
        [JsonPropertyName("qty")]
        public int Quantity { get; set; }
    }
}
