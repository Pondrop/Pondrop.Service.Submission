using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.Submission.Domain.Enums.Campaign;

[JsonConverter(typeof(RepeatEveryUOMEnumConverter))]
public enum RepeatEveryUOM
{
    mins, hours, days, weeks, months, years
}


internal class RepeatEveryUOMEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (string.IsNullOrEmpty(reader?.Value?.ToString()))
            return null;

        return base.ReadJson(reader, objectType, existingValue, serializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}