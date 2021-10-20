using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Unit selling machine (dispensing locker)
    /// </summary>
    public abstract class Machine : LayoutUnit, IMachine
    {
        public IEnumerable<ITray> Trays { get => _trays; } 

        public ITray AddTray<TTray>(ushort number)
            where TTray: Tray, new()
        {
            TTray result = default;

            if (!_trays.Any() || !_trays.Any(x => x.Number == number))
            {
                result = new TTray();
                result.SetNumber(number);
                (result as Tray).SetMachine(this);
                _trays.Add(result);
            }
            else if (_trays.Any(x => x.Number == number))
                throw new ArgumentException($"A tray with number {number} already exists!");

            return result;
        }

        private ICollection<ITray> _trays = new List<ITray>();

        public override string ToString() => $"{this.GetType().Name} {Number} consists of {Trays.Count()} tray(s)";
    }
}