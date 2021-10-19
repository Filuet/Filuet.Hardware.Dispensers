using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models
{
    /// <summary>
    /// Planogram
    /// </summary>
    public class PoG
    {
        public PoGProduct this[string produitUid] => Products.FirstOrDefault(x => string.Equals(x.ProductUid, produitUid, System.StringComparison.InvariantCultureIgnoreCase));

        public IEnumerable<PoGProduct> Products { get; set; }

        public static PoG Read(string serialized)
            => new PoG { Products = JsonConvert.DeserializeObject<IEnumerable<PoGProduct>>(serialized) };

        [JsonIgnore]
        public IEnumerable<CompositDispenseAddress> Addresses
            => Products.SelectMany(x => x.Addresses);
    }

    public class PoGProduct
    {
        [JsonProperty("productUid")]
        public string ProductUid { get; private set; }

        [JsonProperty("machines")]
        public IEnumerable<PoGMachine> Machines { get; private set; }

        [JsonIgnore]
        public IEnumerable<CompositDispenseAddress> Addresses
            => Machines.SelectMany(x => x.Addresses.Select(a => CompositDispenseAddress.Create(x.MachineId, a)));
    }

    public class PoGMachine
    {
        [JsonProperty("id")]
        public uint MachineId { get; private set; }

        [JsonProperty("addresses")]
        public IEnumerable<string> Addresses { get; private set; }
    }
}