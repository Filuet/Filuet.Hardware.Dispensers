using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.Dispenser
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
        private static readonly ConcurrentQueue<CurrentStatus> _statusQueue = new();

        public static void AddStatus(CurrentStatus status)
        {

            if (status != null && !string.IsNullOrWhiteSpace(status.Status))
            {
                _statusQueue.Enqueue(status);
            }
        }

        public static CurrentStatus GetLatestStatus()
        {
            if (_statusQueue.TryDequeue(out var status))
            {
                return status;
            }
            return new CurrentStatus { Action = "pending", Status = "success", Message = "Waiting for command" };
        }

        public static IEnumerable<CurrentStatus> GetAllStatuses()
        {
            return _statusQueue.ToArray();
        }

        public static void ClearStatuses()
        {
            while (_statusQueue.TryDequeue(out _)) { }
        }
    }

}
