using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions.Helpers
{
    public static class PlanogramHelper
    {
        /// <summary>
        /// Retrieves products that are about to run out of stock
        /// </summary>
        /// <param name="planogram"></param>
        /// <param name="thresholdPercent">Product is considered as running low under or equal this value</param>
        /// <returns>List of product identifiers (sku)</returns>
        public static IEnumerable<(string productUid, int count, int maxCount)> GetStock(this Pog planogram) {
            foreach (var product in planogram.Products)
                yield return (product.Product, product.Quantity, product.MaxQuantity);
        }
    }
}