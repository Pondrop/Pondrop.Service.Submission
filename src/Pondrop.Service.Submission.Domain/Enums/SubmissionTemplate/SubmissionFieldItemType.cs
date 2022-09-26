using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

[JsonConverter(typeof(SubmissionFieldSearchTypeEnumConverter))]
public enum SubmissionFieldItemType
{
    unknown,
    products,
}

internal class SubmissionFieldSearchTypeEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (string.IsNullOrEmpty(reader?.Value?.ToString()))
            return SubmissionFieldItemType.unknown;

        return base.ReadJson(reader, objectType, existingValue, serializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}