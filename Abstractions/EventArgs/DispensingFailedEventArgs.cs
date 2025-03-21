namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispensingFailedEventArgs : DispenseSessionEventArgs
    {
        public string address { get; set; }

        public string Sku { get; set; }

        public bool emptyBelt { get; set; }

        public string message { get; set; }

        public override string ToString()
            => $"{(string.IsNullOrWhiteSpace(address) ? string.Empty : $"[{address}] ")}{GetType().Name}: Reason:{message} for sku:{(string.IsNullOrWhiteSpace(Sku) ? string.Empty : Sku.Trim())}";
    }
}