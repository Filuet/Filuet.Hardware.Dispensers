using System.Text.Json.Serialization;

namespace ExpoExtractor
{
    public class ProductStock
    {
        [JsonPropertyName("sku")]
        public string Sku { get; set; }

        [JsonPropertyName("qty")]
        public int Quantity { get; set; }
        [JsonPropertyName("max")]
        public int MaxQuantity { get; set; }
    }
}
