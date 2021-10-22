namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface IBelt : ILayoutUnit
    {
        ITray Tray { get; }
    }
}