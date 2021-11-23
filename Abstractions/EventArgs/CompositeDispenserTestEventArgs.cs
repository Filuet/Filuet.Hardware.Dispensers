namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class CompositeDispenserTestEventArgs : DispenserTestEventArgs
    {
        public IDispenser Dispenser { get; set; }
    }
}