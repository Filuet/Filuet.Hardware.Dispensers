namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispenseEventArgs : DispenseSessionEventArgs
    {
        /// <summary>
        /// One item was dispensed from the address
        /// </summary>
        public string address { get; set; }
        public string message { get; set; }

        public static DispenseEventArgs DispensingStarted(string address, string sessionId)
            => new DispenseEventArgs { address = address, sessionId = sessionId , message = "dispensing started" };        
        
        public static DispenseEventArgs DispensingFinished(string address, string sessionId)
            => new DispenseEventArgs { address = address, sessionId = sessionId , message = "dispensing finished" };

        public override string ToString() => $"[{address}] {GetType().Name} {message}";
    }
}