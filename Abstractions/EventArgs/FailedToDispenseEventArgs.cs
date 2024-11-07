using System;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class FailedToDispenseEventArgs : EventArgs
    {
        /// <summary>
        /// sku/quantity
        /// </summary>
        public Dictionary<string, int> ProductsNotGivenFromAddresses { get; set; }
    }
}