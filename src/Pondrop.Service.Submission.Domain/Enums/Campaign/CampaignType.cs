using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.Submission.Domain.Enums.Campaign;

[JsonConverter(typeof(CampaignTypeEnumConverter))]
public enum CampaignType
{
    task,
    orchestration
}


internal class CampaignTypeEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (string.IsNullOrEmpty(reader?.Value?.ToString()))
            return CampaignStatus.draft;

        return base.ReadJson(reader, objectType, existingValue, serializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}