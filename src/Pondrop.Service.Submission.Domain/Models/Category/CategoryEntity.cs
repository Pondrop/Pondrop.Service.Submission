using Newtonsoft.Json;
using Pondrop.Service.Submission.Domain.Events;

namespace Pondrop.Service.Submission.Domain.Models;

public record CategoryEntity : EventEntity
{
    public CategoryEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        Type = string.Empty;
        PublicationLifecycleId = string.Empty;
    }

    public CategoryEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; private set; }

    [JsonProperty(PropertyName = "publicationLifecycleID")]
    public string PublicationLifecycleId { get; private set; }

    public override void Apply(IEventPayload eventPayloadToApply, string createdBy) => throw new NotImplementedException();
    protected override void Apply(IEvent eventToApply) => throw new NotImplementedException();
}