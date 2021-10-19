using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Text.Json;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Tests.Entities
{
    public class VisionEspBelt : Belt {
        public override string Address => JsonSerializer.Serialize(new { T = Tray.Number, B = Number });
    }
}