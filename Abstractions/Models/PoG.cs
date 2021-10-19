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
            => new PoG { Products = JsonSerializer.Deserialize<IEnumerable<PoGProduct>>(serialized) };

        [JsonIgnore]
        public IEnumerable<CompositDispenseAddress> Addresses
            => Products.SelectMany(x => x.Addresses);
    }

    public class PoGProduct
    {
        [JsonPropertyName("productUid")]
        public string ProductUid { get; private set; }

        [JsonPropertyName("machines")]
        public IEnumerable<PoGMachine> Machines { get; private set; }

        [JsonIgnore]
        public IEnumerable<CompositDispenseAddress> Addresses
            => Machines.SelectMany(x => x.Addresses.Select(a => CompositDispenseAddress.Create(x.MachineId, a)));
    }

    public class PoGMachine
    {
        [JsonPropertyName("id")]
        public uint MachineId { get; private set; }

        [JsonPropertyName("addresses")]
        public IEnumerable<string> Addresses { get; private set; }
    }
}