using System.Text.Json.Serialization;

namespace ExpoExtractor
{
    public class CurrentStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class StatusSingleton
    {
        public static CurrentStatus Status { get; set; }
    }
}
