using System;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// A command for <see cref="ICompositeDispenser"/> to dispense a product
    /// </summary>
    public class DispenseCommand
    {
        public PoGRoute Route { get; private set; }

        public ushort Quantity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        /// <param name="quantity">Quantity to be extracted</param>
        /// <returns></returns>
        public static DispenseCommand Create(PoGRoute route, ushort quantity)
        {
            if (quantity == 0)
                throw new ArgumentException("Quantity to dispense is mandatory");

            return new DispenseCommand { Route = route, Quantity = quantity };
        }
    }
}