using Filuet.Hardware.Dispensers.Abstractions.Models;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Tests.Entities
{
    public class VisionEspBelt : Belt
    {
        public override DispensingRoute Address => base.Address;
    }
}