using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using Filuet.Hardware.Dispensers.Core.Strategy;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Communication;
using Filuet.Infrastructure.Communication;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(PoG.Read(File.ReadAllText("test_planogram.json")))
    .AddVendingMachine(sp => {
        ICollection<ILightEmitter> integratedEmitters = new List<ILightEmitter>();
        IVendingMachine vendingMachine = new VendingMachineBuilder()
            .AddChainBuilder(new DispensingChainBuilder(sp.GetRequiredService<PoG>()))
            .AddDispensers(() => {
                string jsonSettings = File.ReadAllText("dispensing_settings.json");
                var machineSettings = JsonSerializer.Deserialize<IEnumerable<VisionEsPlusSettings>>(jsonSettings);
                List<IDispenser> result = new List<IDispenser>();
                int id = 0;
                foreach (var curSettings in machineSettings) {
                    ICommunicationChannel channel = null;
                    if (curSettings.IpOrSerialAddress.Contains("COM"))
                        channel = new EspSerialChannel(s => { s.PortName = curSettings.IpOrSerialAddress; });
                    else
                        channel = new EspTcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(curSettings.IpOrSerialAddress), curSettings.PortNumber); });

                    VisionEsPlusWrapper machine = new VisionEsPlusWrapper((uint)++id, new VisionEsPlus(channel, curSettings));
                    result.Add(machine);
                    integratedEmitters.Add(machine);
                }

                return result;
            })
            .AddLightEmitters(() => integratedEmitters)
            .AddPlanogram(sp.GetRequiredService<PoG>())
            .Build();

        vendingMachine.onDispensing += (sender, e) => Console.WriteLine($"Dispensing is started {e.address}");
        vendingMachine.onDispensed += (sender, e) => Console.WriteLine($"Dispensing is finished {e}");
        vendingMachine.onAbandonment += (sender, e) => Console.WriteLine($"Likely that products were abandoned {e}");
        vendingMachine.onFailed += (sender, e) => Console.WriteLine(e.ToString());
        vendingMachine.onLightsChanged += (sender, e) => Console.WriteLine($"{e.Alias} Lights are {(e.IsOn ? "On" : "Off")}");
        vendingMachine.onPlanogramClarification += (sender, e) => {
            //form.Planogram = e.Planogram;
            //form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"The planogram is downloaded");
        };
        // vendingMachine.onTest += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, e.Message);
        ////vendingMachine.onResponse += (sender, e) => Console.WriteLine($"{sender}: {e}");

        return vendingMachine;
    }, null);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
