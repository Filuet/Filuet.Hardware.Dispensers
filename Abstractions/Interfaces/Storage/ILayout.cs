using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface ILayout
    {
        IBelt GetBelt(string address);

        IEnumerable<IBelt> GetBelts(IEnumerable<DispensingRoute> addresses, bool activeOnly = true);

        IEnumerable<IMachine> Machines { get; }

        IMachine AddMachine<TMachine>(ushort number)
            where TMachine : Machine, new();

        ILayout AddMachines(IEnumerable<IMachine> machines);
    }
}