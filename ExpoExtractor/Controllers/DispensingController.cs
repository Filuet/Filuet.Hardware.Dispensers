using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispenser.Controllers
{
    [ApiController]
    [Route("dispensing")]
    public class DispensingController : ControllerBase
    {
        public DispensingController(IVendingMachine vendingMachine, Pog planogram, IConfiguration configuration, ILogger<DispensingController> logger, PlanogramService planogramService)
        {
            _vendingMachine = vendingMachine;
            _planogram = planogram;
            _configuration = configuration;
            _logger = logger;
            _planogramService = planogramService;
            
            // Subscribe to the event only once in constructor
            _vendingMachine.onTest += OnVendingMachineTest;
        }

        private void OnVendingMachineTest(object? sender, VendingMachineTestEventArgs e)
        {
            if (e?.Dispenser == null)
            {
                _logger.LogError("Received onTest event with null Dispenser. Sender: {Sender}, Message: {Message}, Severity: {Severity}", 
                    sender?.GetType()?.Name ?? "null", e?.Message ?? "null", e?.Severity.ToString() ?? "null");
                return;
            }

            var testResult = new MachineTestResult { Machine = e.Dispenser.Id, Status = e.Message };
            
            // Store results in thread-safe concurrent dictionary, keyed by a unique request identifier
            foreach (var kvp in _activeTestRequests.ToList())
            {
                var requestResults = kvp.Value;
                if (requestResults != null)
                {
                    // Remove any existing result for this machine and add the new one
                    requestResults.AddOrUpdate(e.Dispenser.Id, testResult, (key, oldValue) => testResult);
                }
            }
        }

        [HttpPost("extract")]
        public async Task Extract(IEnumerable<ExtractSlot> toDispense)
        {
            StatusSingleton.ClearStatuses();
            await _vendingMachine.DispenseAsync(new Cart(toDispense.Select(x => new CartItem { Sku = x.Sku, Quantity = x.Quantity })));
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var status = StatusSingleton.GetLatestStatus();
            if (status != null)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss}: Status requested. Current status is {status.Action}");
                _logger.LogInformation($"{DateTime.Now:HH:mm:ss}: Status requested. Current status is {status.Action}");
            }

            string result = JsonSerializer.Serialize(status);
            return Ok(result);
        }

        [HttpGet("all-statuses")]
        public IActionResult AllStatuses()
        {
            var statuses = StatusSingleton.GetAllStatuses();
            string result = JsonSerializer.Serialize(statuses);
            return Ok(result);
        }

        [HttpPost("reset-status")]
        public IActionResult ResetStatus()
        {
            StatusSingleton.ClearStatuses();
            _logger.LogInformation("Status has been reset to default.");
            return Ok("Status has been reset to default.");
        }

        [HttpGet("stock")]
        public IEnumerable<ProductStock> Stock()
            => _planogram.Products.Select(x => new ProductStock
            {
                Sku = x.Product,
                Quantity = x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => (int)r.Quantity).Sum(),
                MaxQuantity = x.Routes.Where(r => r.Active.HasValue && r.Active.Value).Select(r => (int)r.MaxQuantity).Sum(),
            });

        [HttpGet("test")]
        public async Task<IEnumerable<MachineTestResult>> Test()
        {
            var requestId = Guid.NewGuid();
            var requestResults = new ConcurrentDictionary<int, MachineTestResult>();
            
            // Register this request's result collection
            _activeTestRequests[requestId] = requestResults;
            
            try
            {
                await _vendingMachine.TestAsync();
                
                // Wait a brief moment for all events to be processed
                await Task.Delay(100);
                
                return requestResults.Values.OrderBy(x => x.Machine);
            }
            finally
            {
                // Clean up this request's result collection
                _activeTestRequests.TryRemove(requestId, out _);
            }
        }

        [HttpGet("unlock/{machine}")]
        public void Unlock(int machine)
            => _vendingMachine.Unlock(machine);

        [HttpPost("planogram")]
        public IActionResult UpdatePlanogram([FromBody] RouteUpdateRequest planogramUpdate)
        {
            if (string.IsNullOrWhiteSpace(planogramUpdate.Address))
                return BadRequest("Address is mandatory");

            if (!planogramUpdate.MaxQty.HasValue || !planogramUpdate.Qty.HasValue || !planogramUpdate.IsActive.HasValue)
            {
                PogRoute route = _planogram.GetRoute(planogramUpdate.Address);
                if (route != null)
                {
                    planogramUpdate.Qty = planogramUpdate.Qty ?? route.Quantity;
                    planogramUpdate.MaxQty = planogramUpdate.MaxQty ?? route.MaxQuantity;
                    planogramUpdate.IsActive = planogramUpdate.IsActive ?? route.Active;
                }
                else throw new ArgumentException("Product quantity, max quantity and active flag are mandatory");
            }

            if (planogramUpdate.Qty > planogramUpdate.MaxQty)
                return BadRequest("Wrong max quantity");

            _planogram.UpdateRoute(new PogRoute
            {
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
        private readonly ILogger<DispensingController> _logger;
        private readonly PlanogramService _planogramService;
        
        // Thread-safe storage for test results, keyed by request ID
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, MachineTestResult>> _activeTestRequests = new();
    }
}
