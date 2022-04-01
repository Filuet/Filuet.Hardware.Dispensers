namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class VendingMachineTestEventArgs : DispenserTestEventArgs
    {
        public IDispenser Dispenser { get; set; }
    }
}