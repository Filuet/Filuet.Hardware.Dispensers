namespace Filuet.Hardware.Dispensers.Abstractions.Enums
{
    public enum DoorState : byte
    {
        Unknown = 0,
        DoorOpen = 0x4F,
        DoorClosed = 0x43
    }
}