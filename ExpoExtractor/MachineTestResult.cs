using System.Text.Json.Serialization;

namespace ExpoExtractor
{
    public class MachineTestResult
    {
        [JsonPropertyName("machine")]
        public int Machine { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
