using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Converters
{
    public class DispensingRouteConverter : JsonConverter<DispensingRoute>
    {
        public override DispensingRoute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            string[] x = value.Trim().Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (x.Length != 3)
                throw new ArgumentException($"Invalid route detected {value}");

            if (!ushort.TryParse(x[0], out ushort machineId))
                throw new ArgumentException($"Invalid machine id {value}");

            return DispensingRoute.Create(machineId, $"{x[1]}/{x[2]}");
        }

        public override void Write(
            Utf8JsonWriter writer,
            DispensingRoute route,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(route);
    }
}