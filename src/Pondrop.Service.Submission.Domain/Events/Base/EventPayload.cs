using Newtonsoft.Json;

namespace Pondrop.Service.Submission.Domain.Events;

public record EventPayload : IEventPayload
{
    public DateTime CreatedUtc { get; } = DateTime.UtcNow;
}