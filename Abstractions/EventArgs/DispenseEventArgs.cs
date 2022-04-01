using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispenseEventArgs : EventArgs
    {
        public string address { get; set; } // One item was dispensed from the address

        public static DispenseEventArgs Create(string address)
            => new DispenseEventArgs { address = address };

        public override string ToString() => $"{GetType().Name}({address})";
    }
}