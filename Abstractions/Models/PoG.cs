using Filuet.Hardware.CashAcceptors.Abstractions.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Planogram
    /// </summary>
    public class PoG
    {
        public PoGProduct this[string produitUid] => Products.FirstOrDefault(x => string.Equals(x.ProductUid, produitUid, System.StringComparison.InvariantCultureIgnoreCase));

        public IEnumerable<PoGProduct> Products { get; set; }

        public static PoG Read(string serialized)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { };
            options.Converters.Add(new DispensingRouteConverter());

            return new PoG { Products = JsonSerializer.Deserialize<IEnumerable<PoGProduct>>(serialized, options) };
        }

        [JsonIgnore]
        public IEnumerable<DispensingRoute> Addresses => Products.SelectMany(x => x.Routes.Select(r => r.Route));
    }

    public class PoGProduct
    {
        [JsonPropertyName("product")]
        public string ProductUid { get; set; }

        [JsonPropertyName("routes")]
        public IEnumerable<PoGRoute> Routes { get; set; }

        [JsonIgnore]
        public IEnumerable<DispensingRoute> Addresses => Routes.Select(x => x.Route);
    }

    public class PoGRoute
    {
        [JsonPropertyName("r")]
        public DispensingRoute Route { get; set; }

        [JsonPropertyName("q")]
        public ushort Quantity { get; set; }
    }
}