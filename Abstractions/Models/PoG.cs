using Filuet.Infrastructure.Abstractions.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Planogram
    /// </summary>
    public class PoG
    {
        public PoGProduct this[string productUid] => Products.FirstOrDefault(x => string.Equals(x.ProductUid, productUid, StringComparison.InvariantCultureIgnoreCase));

        public PoGRoute GetRoute(string address)
            => Products.SelectMany(x => x.Routes).FirstOrDefault(x => address == x.Address);

        public IEnumerable<PoGRoute> GetRoutes(IEnumerable<string> addresses)
            => Products.SelectMany(x => x.Routes).Where(x => addresses.Any(r => r == x.Address));

        public IEnumerable<PoGRoute> GetRoutes(uint machine)
            => Products.SelectMany(x => x.Routes).Where(x => x.Dispenser?.Id == machine);

        public PoGProduct GetProduct(string address)
            => Products.FirstOrDefault(x => x.Addresses.Contains(address));

        [JsonPropertyName("products")]
        public ICollection<PoGProduct> Products { get; set; }

        [JsonIgnore]
        public IEnumerable<string> Addresses => Products?.SelectMany(x => x.Routes.Select(r => r.Address)).OrderBy(x => x.Length).ThenBy(x => x).ToList();

        [JsonIgnore]
        public IEnumerable<string> ActiveAddresses => Products?.SelectMany(x => x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => r.Address)).OrderBy(x => x.Length).ThenBy(x => x).ToList();

        public static PoG Read(string serialized) {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BoolToNumJsonConverter());
            return new PoG { Products = JsonSerializer.Deserialize<List<PoGProduct>>(serialized, options) };
        }

        public void UpdateRoute(PoGRoute route, string productUid) {
            if (string.IsNullOrWhiteSpace(productUid))
                throw new ArgumentException("Product UID is mandatory");

            if (route == null || string.IsNullOrWhiteSpace(route.Address))
                throw new ArgumentException("Address is mandatory");

            productUid = productUid.Trim();

            bool needToAdd = true;

            List<PoGProduct> toRemove = new List<PoGProduct>();

            foreach (var p in Products) {
                PoGRoute existedRoute = p.Routes.FirstOrDefault(x => x.Address == route.Address);
                if (existedRoute != null) {
                    if (string.Equals(p.ProductUid, productUid, StringComparison.InvariantCultureIgnoreCase)) {
                        existedRoute.Quantity = route.Quantity;
                        existedRoute.MaxQuantity = route.MaxQuantity;
                        existedRoute.Active = route.Active;
                        needToAdd = false;
                    }
                    else {
                        p.Routes.Remove(existedRoute);
                        if (!p.Routes.Any())
                            toRemove.Add(p);
                    }
                }

                p.Routes = p.Routes.OrderBy(x => x.Address).ToList();
            }

            if (needToAdd) {
                PoGProduct p = this[productUid];
                if (p != null)
                    p.Routes.Add(route);
                else Products.Add(new PoGProduct { ProductUid = productUid, Routes = new List<PoGRoute> { route } });
            }

            foreach (var p in toRemove)
                Products.Remove(p);
        }

        public void RemoveRoute(PoGRoute route) {
            PoGProduct toDelete = null;
            foreach (var p in Products) {
                PoGRoute existedRoute = p.Routes.FirstOrDefault(x => x.Address == route.Address);
                if (existedRoute != null) {
                    p.Routes.Remove(existedRoute);
                    if (p.Routes.Count == 0)
                        toDelete = p;
                    break;
                }
            }
            if (toDelete != null)
                Products.Remove(toDelete);
        }

        public void SetAttributes(string route, IDispenser dispenser, bool available) {
            foreach (var p in Products)
                foreach (var r in p.Routes)
                    if (r.Address == route) {
                        r.Active = dispenser != null && available;
                        r.Dispenser = dispenser;
                        break;
                    }
        }

        public string ToString(bool writeIndented = true) {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BoolToNumJsonConverter());
            options.WriteIndented = writeIndented;
            return JsonSerializer.Serialize(Products.OrderBy(x => x.ProductUid), options);
        }
    }

    public class PoGProduct
    {
        [JsonPropertyName("product")]
        public string ProductUid { get; set; }

        [JsonPropertyName("routes")]
        public ICollection<PoGRoute> Routes { get; set; }

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

        [JsonPropertyName("m")]
        public ushort MaxQuantity { get; set; } = 10;

        [JsonPropertyName("a")]
        [JsonConverter(typeof(BoolToNumJsonConverter))]
        public bool? Active { get; set; }

        [JsonIgnore]
        public IDispenser Dispenser { get; set; }

        public override string ToString() => $"{Address}: {Quantity}";
    }
}