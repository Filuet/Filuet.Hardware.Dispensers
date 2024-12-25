using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public class RouteUpdateRequest
    {
        [JsonPropertyName("route")]
        public string Address { get; set; }
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
        [JsonPropertyName("qty")]
        public int? Qty { get; set; }
        [JsonPropertyName("maxQty")]
        public int? MaxQty { get; set; }
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }
    }
}
