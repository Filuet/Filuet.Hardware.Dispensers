using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispenseFailEventArgs : EventArgs
    {
        public string address { get; set; }

        public bool emptyBelt { get; set; }

        public string message { get; set; }

        public override string ToString()
            => $"{GetType().Name}({address}) {message}";
    }
}