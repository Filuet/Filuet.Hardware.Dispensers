using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public abstract class DispenseSessionEventArgs : EventArgs
    {
        public string sessionId { get; set; }
    }
}