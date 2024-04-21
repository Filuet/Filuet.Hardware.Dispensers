using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExpoExtractor.Controllers
{
    [ApiController]
    [Route("dispensing")]
    public class DispensingController : ControllerBase
    {
        public DispensingController(IVendingMachine vendingMachine, PoG planogram, ILogger<DispensingController> logger) {
            _vendingMachine = vendingMachine;
            _planogram = planogram;
            _logger = logger;
            _vendingMachine.onTest += (sender, e) => {
                if (_message == null)
                    _message = new List<MachineTestResult>();

                _message.RemoveAll(x => x.Machine == e.Dispenser.Alias);
                _message.Add(new MachineTestResult { Machine = e.Dispenser.Alias, Status = e.Message });
            };
        }

        [HttpPost("extract")]
        public async Task Extract(IEnumerable<ExtractSlot> toDispense)
            => await _vendingMachine.Dispense(toDispense.Select(x => (x.Sku, (ushort)x.Quantity)).ToArray());

        [HttpGet("status")]
        public IActionResult Status() {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss}: Status requested. Current status is {StatusSingleton.Status.Action}");

            if (StatusSingleton.Status == null || string.IsNullOrWhiteSpace(StatusSingleton.Status.Status))
                StatusSingleton.Status = new CurrentStatus { Action = "pending", Status = "success", Message = "Waiting for command" };

            string result = JsonSerializer.Serialize(StatusSingleton.Status);

            if ((StatusSingleton.Status.Action == "dispensing" || StatusSingleton.Status.Action == "takeproducts") && StatusSingleton.Status.Status == "success")
                return Ok(result);

            if (StatusSingleton.Status.Action == "dispensed") {
                StatusSingleton.Status = new CurrentStatus { Action = "pending", Status = "success", Message = "Waiting for command" };
                return Ok(result);
            }

            if (StatusSingleton.Status.Action != "pending")
                StatusSingleton.Status = new CurrentStatus { Action = "pending", Status = "success", Message = "Waiting for command" };

            return Ok(result);
        }


        [HttpGet("stock")]
        public IEnumerable<ProductStock> Stock()
            => _planogram.Products.Select(x => new ProductStock {
                Sku = x.ProductUid,
                Quantity = x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => (int)r.Quantity).Sum(),
                MaxQuantity = x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => (int)r.MaxQuantity).Sum(),
            });

        [HttpGet("test")]
        public async Task<IEnumerable<MachineTestResult>> Test() {
            _message = new List<MachineTestResult>();
            await _vendingMachine.Test();

            return _message.OrderBy(x => x.Machine);
        }

        [HttpGet("unlock/{machine}")]
        public void Unlock(int machine)
            => _vendingMachine.Unlock((uint)machine);

        private readonly IVendingMachine _vendingMachine;
        private readonly PoG _planogram;
        private List<MachineTestResult> _message;
        private readonly ILogger<DispensingController> _logger;
    }
}
