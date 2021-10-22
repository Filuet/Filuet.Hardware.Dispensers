using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Converters
{
    public class BoolToNumJsonConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetInt32() != 0;

        public override void Write(
            Utf8JsonWriter writer,
            bool route,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(route ? "1" : "0");
    }
}