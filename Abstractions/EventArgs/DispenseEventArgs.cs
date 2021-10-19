using System;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions
{
    public class DispenseEventArgs : EventArgs
    {
        public string address { get; set; }

        public override string ToString()
            => $"{GetType().Name}({address})";
    }
}
