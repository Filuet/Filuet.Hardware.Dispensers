using System.Text.Json.Serialization;

namespace ExpoExtractor
{
    public class MachineTestResult
    {
        [JsonPropertyName("machine")]
        public string Machine { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
