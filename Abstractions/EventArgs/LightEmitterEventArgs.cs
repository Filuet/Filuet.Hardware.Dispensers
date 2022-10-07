using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class LightEmitterEventArgs : EventArgs
    {
        /// <summary>
        /// Emitter id
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Emitter alias
        /// </summary>
        public string Alias { get; set; }

        public bool IsOn { get; set; }

        public static LightEmitterEventArgs Create(uint id, string alias, bool isOn)
            => new LightEmitterEventArgs { Id = id, Alias = alias, IsOn = isOn };

        public override string ToString() => $"{GetType().Name}({IsOn})";
    }
}