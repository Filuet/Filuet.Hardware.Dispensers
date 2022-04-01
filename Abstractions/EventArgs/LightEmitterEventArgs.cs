using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class LightEmitterEventArgs : EventArgs
    {
        public uint Id { get; set; }

        public bool IsOn { get; set; }

        public static LightEmitterEventArgs Create(uint id, bool isOn)
            => new LightEmitterEventArgs { Id = id, IsOn = isOn };

        public override string ToString() => $"{GetType().Name}({IsOn})";
    }
}