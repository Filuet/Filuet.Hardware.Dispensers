using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispenser
{
    public class MachineTestResult
    {
        [JsonPropertyName("machine")]
        public int Machine { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
