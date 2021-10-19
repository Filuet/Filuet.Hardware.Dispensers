using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models
{
    /// <summary>
    /// Unit selling machine (dispensing locker)
    /// </summary>
    public class Layout : ILayout
    {
        public IEnumerable<IMachine> Machines { get => _machines; }

        public IMachine AddMachine<TMachine>(uint number)
            where TMachine : Machine, new()
        {
            IMachine result = default;

            if (!_machines.Any() || !_machines.Any(x => x.Number == number))
            {
                result = new TMachine();
                result.SetNumber(number);
                _machines.Add(result);
            }
            else if (_machines.Any(x => x.Number == number))
                throw new ArgumentException($"A machine with number {number} already exists!");

            return result;
        }

        private ICollection<IMachine> _machines = new List<IMachine>();

        public ILayout AddMachines(IEnumerable<IMachine> machines)
        {
            _machines = machines.ToList();
            return this;
        }

        public override string ToString() => $"{Machines.Count()} machines";
    }
}