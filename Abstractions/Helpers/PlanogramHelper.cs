using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
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
        public static IEnumerable<string> GetRunningLowProducts(this PoG planogram, int thresholdPercent) {
            foreach (var product in planogram.Products) {
                if (Math.Round(product.Quantity / (decimal)product.MaxQuantity, 2) <= Math.Round(thresholdPercent / 100m, 2)) {
                    yield return product.ProductUid;
                }
            }
        }
    }
}
