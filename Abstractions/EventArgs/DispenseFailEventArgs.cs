using System;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions
{
    public class DispenseFailEventArgs : EventArgs
    {
        public string address { get; set; }

        public string message { get; set; }

        public override string ToString()
            => $"{GetType().Name}({address}) {message}";
    }
}
