using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

[JsonConverter(typeof(SubmissionFieldTypeEnumConverter))]
public enum SubmissionFieldType
{
    unknown,
    photo,
    text,
    multilineText,
    integer,
    currency,
    picker,
    search,
    focus,
    date,
    barcode,
}

internal class SubmissionFieldTypeEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            if (string.IsNullOrEmpty(reader?.Value?.ToString()))
                return SubmissionFieldType.unknown;

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch
        {
            return SubmissionFieldType.unknown;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}