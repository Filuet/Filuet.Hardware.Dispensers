namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispenseFailedEventArgs : DispenseSessionEventArgs
    {
        public string address { get; set; }

        public bool emptyBelt { get; set; }

        public string message { get; set; }

        public override string ToString()
            => $"{(string.IsNullOrWhiteSpace(address) ? string.Empty : $"[{address}] ")}{GetType().Name}: {message}";
    }
}