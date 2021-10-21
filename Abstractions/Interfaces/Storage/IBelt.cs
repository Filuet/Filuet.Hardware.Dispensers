using Filuet.Hardware.Dispensers.Abstractions.Models;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface IBelt : ILayoutUnit {
        DispensingRoute Address { get; }
    }
}