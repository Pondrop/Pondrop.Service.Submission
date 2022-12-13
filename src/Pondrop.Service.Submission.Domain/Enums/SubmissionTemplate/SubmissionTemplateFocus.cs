using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

[JsonConverter(typeof(SubmissionTemplateFocusEnumConverter))]
public enum SubmissionTemplateFocus
{
    unknown,
    product,
    category
}

internal class SubmissionTemplateFocusEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            if (string.IsNullOrEmpty(reader?.Value?.ToString()))
                return SubmissionTemplateFocus.unknown;

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch
        {
            return SubmissionTemplateFocus.unknown;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}