using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispenseEventArgs : EventArgs
    {
        public string address { get; set; }

        public override string ToString()
            => $"{GetType().Name}({address})";
    }
}