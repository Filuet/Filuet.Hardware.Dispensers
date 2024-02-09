using Filuet.Hardware.Dispensers.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet("unlock/{machine}")]
        public void Unlock(int machine)
            => _vendingMachine.Unlock((uint)machine);

        private readonly IVendingMachine _vendingMachine;
        private readonly ILogger<DispensingController> _logger;
    }
}
