using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class LightEmitterEventArgs : EventArgs
    {
        /// <summary>
        /// Emitter id
        /// </summary>
        public int Id { get; set; }

        public bool IsOn { get; set; }

        public static LightEmitterEventArgs Create(int id, bool isOn)
            => new LightEmitterEventArgs { Id = id, IsOn = isOn };

        public override string ToString() => $"{GetType().Name}({IsOn})";
    }
}