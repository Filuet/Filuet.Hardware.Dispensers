using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.IO;

using Filuet.Hardware.Dispensers.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Filuet.Hardware.Dispenser
{

    public class PlanogramService
    {
        private Pog _planogram;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PlanogramService> _logger;
        private readonly string _planogramPath;
        private readonly object _lock = new();

        public PlanogramService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<PlanogramService> logger)
        {
            _serviceProvider = serviceProvider; // Store ServiceProvider
            _planogramPath = configuration["PlanogramPath"];
            _logger = logger;

            _logger.LogInformation("PlanogramService initialized. Planogram path: {Path}", _planogramPath);
            LoadPlanogram();
        }

        private void LoadPlanogram()
        {
            lock (_lock)
            {
                if (!File.Exists(_planogramPath))
                {
                    _logger.LogWarning("Planogram file not found: {Path}", _planogramPath);
                    return;
                }

                _planogram = Pog.Read(File.ReadAllText(_planogramPath));
                _logger.LogInformation("Planogram loaded successfully from {Path}", _planogramPath);
            }
        }

        public Pog GetPlanogram()
        {
            lock (_lock)
            {
                return _planogram;
            }
        }

        public void UpdatePlanogram(Pog updatedPlanogram)
        {
            lock (_lock)
            {
                updatedPlanogram.Write(_planogramPath);
                _planogram = updatedPlanogram;

                _logger.LogInformation("Planogram updated. Notifying vending machine...");

                // Resolve IVendingMachine when needed (Lazy Injection)
                var vendingMachine = _serviceProvider.GetRequiredService<IVendingMachine>();
                vendingMachine.UpdatePlanogram(_planogram);

                _logger.LogInformation("Vending machine notified.");
            }
        }
    }


}
