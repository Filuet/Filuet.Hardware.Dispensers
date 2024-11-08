using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public interface ILightEmitter
    {
        event EventHandler<LightEmitterEventArgs> onLightsChanged;

        int Id { get; }
        void LightOn();
        void LightOff();
    }
}