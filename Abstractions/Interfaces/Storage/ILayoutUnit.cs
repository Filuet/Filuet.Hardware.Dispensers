using System;
using System.Collections.Generic;
using System.Text;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface ILayoutUnit
    {
        /// <summary>
        /// Index number
        /// </summary>
        uint Number { get; }

        bool IsActive { get; }

        void SetNumber(uint number);

        void SetActive(bool active);
    }
}
