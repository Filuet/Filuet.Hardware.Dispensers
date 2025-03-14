using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispenser.Controllers
{
    [ApiController]
    [Route("dispensing")]
    public class DispensingController : ControllerBase {
        public DispensingController(IVendingMachine vendingMachine, Pog planogram, IConfiguration configuration, ILogger<DispensingController> logger, PlanogramService planogramService) {
            _vendingMachine = vendingMachine;
            _planogram = planogram;
            _configuration = configuration;
            _logger = logger;
            _planogramService = planogramService;
            _vendingMachine.onTest += (sender, e) => {
                if (_message == null)
                    _message = new List<MachineTestResult>();

                _message.RemoveAll(x => x.Machine == e.Dispenser.Id);
                _message.Add(new MachineTestResult { Machine = e.Dispenser.Id, Status = e.Message });
            };
        }

        [HttpPost("extract")]
        public async Task Extract(IEnumerable<ExtractSlot> toDispense)
            => await _vendingMachine.DispenseAsync(new Cart(toDispense.Select(x => new CartItem { Sku = x.Sku, Quantity = x.Quantity })));

        [HttpGet("status")]
        public IActionResult Status() {
            if (StatusSingleton.Status != null)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss}: Status requested. Current status is {StatusSingleton.Status.Action}");
                _logger.LogInformation($"{DateTime.Now:HH:mm:ss}: Status requested. Current status is {StatusSingleton.Status.Action}");
            }
                


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
                Sku = x.Product,
                Quantity = x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => (int)r.Quantity).Sum(),
                MaxQuantity = x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => (int)r.MaxQuantity).Sum(),
            });

        [HttpGet("test")]
        public async Task<IEnumerable<MachineTestResult>> Test() {
            _message = new List<MachineTestResult>();
            await _vendingMachine.TestAsync();

            return _message.OrderBy(x => x.Machine);
        }

        [HttpGet("unlock/{machine}")]
        public void Unlock(int machine)
            => _vendingMachine.Unlock(machine);

        [HttpPost("planogram")]
        public IActionResult UpdatePlanogram([FromBody] RouteUpdateRequest planogramUpdate) {
            if (string.IsNullOrWhiteSpace(planogramUpdate.Address))
                return BadRequest("Address is mandatory");

            if (!planogramUpdate.MaxQty.HasValue || !planogramUpdate.Qty.HasValue || !planogramUpdate.IsActive.HasValue) {
                PogRoute route = _planogram.GetRoute(planogramUpdate.Address);
                if (route != null) {
                    planogramUpdate.Qty = planogramUpdate.Qty ?? route.Quantity;
                    planogramUpdate.MaxQty = planogramUpdate.MaxQty ?? route.MaxQuantity;
                    planogramUpdate.IsActive = planogramUpdate.IsActive ?? route.Active;
                }
                else throw new ArgumentException("Product quantity, max quantity and active flag are mandatory");
            }

            if (planogramUpdate.Qty > planogramUpdate.MaxQty)
                return BadRequest("Wrong max quantity");

            _planogram.UpdateRoute(new PogRoute {
                Active = planogramUpdate.IsActive,
                Address = planogramUpdate.Address,
                Quantity = (ushort)planogramUpdate.Qty.Value,
                MaxQuantity = (ushort)planogramUpdate.MaxQty.Value
            }, planogramUpdate.Sku);

            string planogramPath = _configuration["PlanogramPath"];

            _planogram.Write(planogramPath);

            return Ok();
        }
        [HttpPost("update-planogram")]
        public IActionResult UpdateWholePlanogram([FromBody] List<PogProduct> updatedProducts)
        {
            if (updatedProducts == null || updatedProducts.Count == 0)
            {
                _logger.LogWarning("Received empty or invalid planogram update request.");
                return BadRequest("Invalid planogram data. Must be a non-empty list.");
            }

            try
            {
                // Load the existing planogram
                Pog updatedPlanogram = _planogramService.GetPlanogram();

                // Replace products in the planogram
                updatedPlanogram.Products = updatedProducts;

                // Save and update planogram
                _planogramService.UpdatePlanogram(updatedPlanogram);

                _logger.LogInformation("Planogram updated successfully.");
                return Ok("Planogram updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating planogram: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the planogram.");
            }
        }


        private readonly IVendingMachine _vendingMachine;
        private readonly IConfiguration _configuration;
        private readonly Pog _planogram;
        private List<MachineTestResult> _message;
        private readonly ILogger<DispensingController> _logger;
        private readonly PlanogramService _planogramService;
    }
}
