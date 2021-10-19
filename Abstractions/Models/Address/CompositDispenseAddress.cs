using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Issuance address
    /// </summary>
    public class CompositDispenseAddress : DispenseAddress
    {
        [JsonPropertyName("vendMachineId")]
        public uint VendingMachineID { get; private set; }

        public static implicit operator String(CompositDispenseAddress address)
            => JsonSerializer.Serialize(address);

        public static CompositDispenseAddress Create(uint vendingMachineId, string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is mandatory");

            return new CompositDispenseAddress { VendingMachineID = vendingMachineId, Address = address.Trim() };
        }

        [JsonIgnore]
        public bool IsAddressAvailable { get; set; }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}