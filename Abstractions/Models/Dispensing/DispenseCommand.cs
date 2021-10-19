using System;
using System.Collections.Generic;
using System.Text;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models
{
    /// <summary>
    /// A command for <see cref="ICompositeDispenser"/> to dispense a product
    /// </summary>
    public class DispenseCommand
    {
        public CompositDispenseAddress Address { get; private set; }

        public uint Quantity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="quantity">Quantity to be extracted</param>
        /// <returns></returns>
        public static DispenseCommand Create(CompositDispenseAddress address, uint quantity)
        {
            if (quantity == 0)
                throw new ArgumentException("Quantity to dispense is mandatory");

            return new DispenseCommand { Address = address, Quantity = quantity };
        }
    }
}
