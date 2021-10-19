using Newtonsoft.Json;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models
{
    /// <summary>
    /// Dispensing address
    /// </summary>
    public abstract class DispenseAddress
    {
        [JsonProperty("address")]
        public string Address { get; protected set; }

        public override string ToString() => Address;
    }
}