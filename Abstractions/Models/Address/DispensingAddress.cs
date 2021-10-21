using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Dispensing address
    /// </summary>
    public abstract class DispensingAddress
    {
        [JsonPropertyName("address")]
        public string Address { get; protected set; }

        public override string ToString() => Address;
    }
}