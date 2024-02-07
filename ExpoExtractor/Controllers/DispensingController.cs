using Filuet.Hardware.Dispensers.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ExpoExtractor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DispensingController : ControllerBase
    {
        public DispensingController(IVendingMachine vendingMachine, ILogger<DispensingController> logger) {
            _vendingMachine = vendingMachine;
            _logger = logger;
        }

        [HttpPost("extract")]
        public async Task Extract(IEnumerable<ExtractSlot> toDispense)
            => await _vendingMachine.Dispense(toDispense.Select(x => (x.Sku, (ushort)x.Quantity)).ToArray());

        private readonly IVendingMachine _vendingMachine;
        private readonly ILogger<DispensingController> _logger;
    }
}
