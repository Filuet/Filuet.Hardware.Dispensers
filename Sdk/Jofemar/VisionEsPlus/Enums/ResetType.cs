namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums
{
    public enum VisionEsPlusResetType
    {
        SoldOutProducts = 0x80,
        WaitingForProductsToBeRemoved = 0x81,
        // Reset of faults + self test.
        MasterReset = 0xff
    }
}
