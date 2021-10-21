using System;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Issuance address
    /// </summary>
    public class DispensingRoute : DispensingAddress
    {
        [JsonPropertyName("vendMachineId")]
        public ushort VendingMachineID { get; private set; }

        public static implicit operator string(DispensingRoute route)
            => route.Address;

        public static DispensingRoute Create(ushort vendingMachineId, string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is mandatory");

            return new DispensingRoute { VendingMachineID = vendingMachineId, Address = $"{vendingMachineId}/{address.Trim()}" };
        }

        public override string ToString() => Address;
    }
}