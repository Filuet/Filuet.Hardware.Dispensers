using Filuet.Infrastructure.Abstractions.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Planogram
    /// </summary>
    public class Pog
    {
        public const int DEFAULT_PRODUCT_WEIGHT_GR = 500;

        public PogProduct this[string productOrAddress]
        {
            get
            {
                Regex regex = new Regex("^[\\d]{1}/[\\d]{2}/[\\d]{1}$");
                if (regex.Match(productOrAddress).Success)
                    return Products.FirstOrDefault(x => x.Routes.Any(r => r.Address == productOrAddress));

                return Products.FirstOrDefault(x => string.Equals(x.Product, productOrAddress, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public PogRoute GetRoute(string address)
            => Products.SelectMany(x => x.Routes).FirstOrDefault(x => address == x.Address);

        public IEnumerable<PogRoute> GetRoutes(IEnumerable<string> addresses)
            => Products.SelectMany(x => x.Routes).Where(x => addresses.Any(r => r == x.Address));

        public PogProduct GetProduct(string address)
            => Products.FirstOrDefault(x => x.Addresses.Contains(address));

        [JsonPropertyName("products")]
        public ICollection<PogProduct> Products { get; set; }

        [JsonIgnore]
        public IEnumerable<string> Addresses => Products?.SelectMany(x => x.Routes.Select(r => r.Address)).OrderBy(x => x.Length).ThenBy(x => x).ToList();

        [JsonIgnore]
        public IEnumerable<string> ActiveAddresses => Products?.SelectMany(x => x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => r.Address)).OrderBy(x => x.Length).ThenBy(x => x).ToList();

        public static Pog Read(string serialized) {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BoolToNumJsonConverter());
            return new Pog { Products = JsonSerializer.Deserialize<List<PogProduct>>(serialized, options) };
        }

        public void Write(string path) {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BoolToNumJsonConverter());
            File.WriteAllText(path, JsonSerializer.Serialize(Products, options));
        }

        public void UpdateRoute(PogRoute route, string productUid) {
            if (string.IsNullOrWhiteSpace(productUid))
                throw new ArgumentException("Product UID is mandatory");

            if (route == null || string.IsNullOrWhiteSpace(route.Address))
                throw new ArgumentException("Address is mandatory");

            productUid = productUid.Trim();

            bool needToAdd = true;

            List<PogProduct> toRemove = new List<PogProduct>();

            foreach (var p in Products) {
                PogRoute existedRoute = p.Routes.FirstOrDefault(x => x.Address == route.Address);
                if (existedRoute != null) {
                    if (string.Equals(p.Product, productUid, StringComparison.InvariantCultureIgnoreCase)) {
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
                PogProduct p = this[productUid];
                if (p != null)
                    p.Routes.Add(route);
                else Products.Add(new PogProduct { Product = productUid, Routes = new List<PogRoute> { route } });
            }

            foreach (var p in toRemove)
                Products.Remove(p);
        }

        public void RemoveRoute(PogRoute route) {
            PogProduct toDelete = null;
            foreach (var p in Products) {
                PogRoute existedRoute = p.Routes.FirstOrDefault(x => x.Address == route.Address);
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

        public void SetAttributes(string route, bool available) {
            foreach (var p in Products)
                foreach (var r in p.Routes)
                    if (r.Address == route) {
                        r.Active = available;
                        break;
                    }
        }

        /// <summary>
        /// Get product weight in gramms
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public int GetProductWeight(string productUid) =>
            this[productUid]?.Weight ?? DEFAULT_PRODUCT_WEIGHT_GR;

        public Pog GetPartialPlanogram(Func<string, bool> isAddressValid) {
            Pog result = new Pog();
            foreach (var p in Products) {
                foreach (var r in p.Routes) {
                    if (isAddressValid(r.Address)) {
                        PogProduct product = result.Products.FirstOrDefault(x => x.Product == p.Product);
                        if (product == null) {
                            product = new PogProduct {
                                Product = p.Product,
                                Weight = p.Weight
                            };
                            result.Products.Add(product);
                        }

                        product.Routes.Add(new PogRoute { Active = r.Active, Address = r.Address, MaxQuantity = r.MaxQuantity, Quantity = r.Quantity });
                    }
                }
            }
            return result;
        }

        public string ToString(bool writeIndented = true) {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BoolToNumJsonConverter());
            options.WriteIndented = writeIndented;
            return JsonSerializer.Serialize(Products.OrderBy(x => x.Product), options);
        }
    }

    public class PogProduct
    {
        [JsonPropertyName("product")]
        public string Product { get; set; }

        [JsonPropertyName("routes")]
        public ICollection<PogRoute> Routes { get; set; }

        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        [JsonIgnore]
        public IEnumerable<string> Addresses => Routes.Select(x => x.Address);

        [JsonIgnore]
        public int Quantity => Routes?.Where(x => x.Active ?? false).Sum(x => x.Quantity) ?? 0;

        [JsonIgnore]
        public int MaxQuantity => Routes?.Where(x => x.Active ?? false).Sum(x => x.MaxQuantity) ?? 0;

        public override string ToString() => Product;
    }

    public class PogRoute : PogRouteMock
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

        [JsonIgnore]
        /// <summary>
        /// Crutch
        /// </summary>
        /// <returns></returns>
        public uint DispenserId => uint.Parse(Address.Substring(0, Address.IndexOf('/')));

        public override string ToString()
            => $"{Address}: {Quantity}";
    }

    public abstract class PogRouteMock
    {
        /// <summary>
        /// Real quantity
        /// </summary>
        [JsonPropertyName("mock_q")]
        public ushort? MockedQuantity { get; set; } = null;

        /// <summary>
        /// Is physically active
        /// </summary>
        [JsonPropertyName("mock_a")]
        public bool? MockedActive { get; set; } = true;
    }
}