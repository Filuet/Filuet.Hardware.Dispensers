using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public interface ILightEmitter
    {
        event EventHandler<LightEmitterEventArgs> onLightsChanged;

        uint Id { get; }

        void LightOn();

        void LightOff();
    }
}