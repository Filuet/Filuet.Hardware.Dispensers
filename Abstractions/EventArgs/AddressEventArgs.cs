namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class AddressEventArgs : DispenseSessionEventArgs
    {
        /// <summary>
        /// One item was dispensed from the address
        /// </summary>
        public string address { get; set; }
        public string message { get; set; }

        public static AddressEventArgs DispensingStarted(string address, string sessionId)
            => new AddressEventArgs { address = address, sessionId = sessionId , message = "dispensing started" };        
        
        public static AddressEventArgs DispensingFinished(string address, string sessionId)
            => new AddressEventArgs { address = address, sessionId = sessionId , message = "dispensing finished" };

        public override string ToString() => $"[{address}] {GetType().Name} {message}";
    }
}