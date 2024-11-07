namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    /// <summary>
    /// Reflects the current stock of the route
    /// </summary>
    public class RouteBalance
    {
        public string Product { get; set; }

        public string Address { get; set; }

        public ushort Quantity { get; set; }

        public bool Active { get; set; } = true;
    }
}