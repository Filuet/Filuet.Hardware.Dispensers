using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface ILayout
    {
        IEnumerable<IMachine> Machines { get; }

        IMachine AddMachine<TMachine>(uint number)
            where TMachine : Machine, new();

        ILayout AddMachines(IEnumerable<IMachine> machines);
    }
}