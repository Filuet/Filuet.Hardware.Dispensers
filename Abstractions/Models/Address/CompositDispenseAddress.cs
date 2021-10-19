using Newtonsoft.Json;
using System;
namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models
{
    /// <summary>
    /// Issuance address
    /// </summary>
    public class CompositDispenseAddress : DispenseAddress
    {
        [JsonProperty("vendMachineId")]
        public uint VendingMachineID { get; private set; }

        public static implicit operator String(CompositDispenseAddress address)
            => JsonConvert.SerializeObject(address);

        public static CompositDispenseAddress Create(uint vendingMachineId, string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is mandatory");

            return new CompositDispenseAddress { VendingMachineID = vendingMachineId, Address = address.Trim() };
        }

        [JsonIgnore]
        public bool IsAddressAvailable { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}