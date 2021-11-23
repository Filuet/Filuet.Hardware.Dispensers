using Filuet.Infrastructure.Abstractions.Converters;
using System;
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

        public PoGRoute GetRoute(string address)
            => Products.SelectMany(x => x.Routes).FirstOrDefault(x => address == x.Address);

        public IEnumerable<PoGRoute> GetRoutes(IEnumerable<string> addresses)
            => Products.SelectMany(x => x.Routes).Where(x => addresses.Any(r => r == x.Address));

        public IEnumerable<PoGRoute> GetRoutes(uint machine)
            => Products.SelectMany(x => x.Routes).Where(x => x.Dispenser?.Id == machine);

        public PoGProduct GetProduct(string address)
            => Products.FirstOrDefault(x => x.Addresses.Contains(address));

        [JsonPropertyName("products")]
        public IEnumerable<PoGProduct> Products { get; set; }

        public static PoG Read(string serialized)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BoolToNumJsonConverter());
            return new PoG { Products = JsonSerializer.Deserialize<List<PoGProduct>>(serialized, options) };
        }

        [JsonIgnore]
        public IEnumerable<string> Addresses => Products.SelectMany(x => x.Routes.Select(r => r.Address)).ToList();

        public void SetAttributes(string route, IDispenser dispenser, bool available)
        {
            foreach (var p in Products)
                foreach (var r in p.Routes)
                    if (r.Address == route)
                    {
                        r.Active = dispenser != null && available;
                        r.Dispenser = dispenser;
                        break;
                    }
        }

        public override string ToString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BoolToNumJsonConverter());
            options.WriteIndented = true;
            return JsonSerializer.Serialize(Products, options);
        }
    }

    public class PoGProduct
    {
        [JsonPropertyName("product")]
        public string ProductUid { get; set; }

        [JsonPropertyName("routes")]
        public IEnumerable<PoGRoute> Routes { get; set; }

        [JsonIgnore]
        public IEnumerable<string> Addresses => Routes.Select(x => x.Address);

        public override string ToString() => ProductUid;
    }

    public class PoGRoute
    {
        [JsonPropertyName("r")]
        public string Address { get; set; }

        [JsonPropertyName("q")]
        public ushort Quantity { get; set; }

        [JsonPropertyName("a")]
        [JsonConverter(typeof(BoolToNumJsonConverter))]
        public bool Active { get; set; }

        [JsonIgnore]
        public IDispenser Dispenser { get; set; }

        public override string ToString() => $"{Address}: {Quantity}";
    }
}