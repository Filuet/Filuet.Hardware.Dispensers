using Filuet.Hardware.Dispensers.Abstractions.Models;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Tests.Entities
{
    public class VisionEspBelt : Belt {
        public override string Address => $"{Tray.Machine.Number}/{Tray.Number}/{Number}";
    }
}