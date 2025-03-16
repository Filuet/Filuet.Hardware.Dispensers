namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class AddressEventArgs : DispenseSessionEventArgs
    {
        /// <summary>
        /// One item was dispensed from the address
        /// </summary>
        public string address { get; set; }
        public string message { get; set; }

        public static AddressEventArgs DispensingStarted(string address, string sessionId, string skuName)
            => new AddressEventArgs { address = address, sessionId = sessionId, message = $"{address} dispensing started for sku:{skuName.Trim()}" };
        public static AddressEventArgs DispensingFinished(string address, string sessionId, string skuName)
            => new AddressEventArgs { address = address, sessionId = sessionId, message = $"{address} dispensing finished for sku:{skuName.Trim()}" };
        public static AddressEventArgs Abandonment(string address, string sessionId, string skuName)
           => new AddressEventArgs
           {
               address = address,
               sessionId = sessionId,
               message = $"Likely that products were abandoned for sku:{skuName.Trim()}"
           };

        public override string ToString() => $"[{address}] {GetType().Name} {message}";
    }
}