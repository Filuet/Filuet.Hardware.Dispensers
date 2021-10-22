using Filuet.Hardware.Dispensers.Abstractions.Interfaces;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusLayoutRouteConverter : ILayoutRouteConverter
    {
        public string GetRoute(IBelt belt) => $"{belt.Tray.Machine.Number}/{belt.Tray.Number}/{belt.Number}";
    }
}
