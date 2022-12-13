using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

[JsonConverter(typeof(SubmissionTemplateStatusEnumConverter))]
public enum SubmissionTemplateStatus
{
    unknown,
    active,
    inactive,
    draft
}

internal class SubmissionTemplateStatusEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            if (string.IsNullOrEmpty(reader?.Value?.ToString()))
                return SubmissionTemplateStatus.unknown;

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch
        {
            return SubmissionTemplateStatus.unknown;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}