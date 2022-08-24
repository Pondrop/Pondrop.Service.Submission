using Newtonsoft.Json.Linq;

namespace Pondrop.Service.Submission.Domain.Events;

public interface IEventPayload
{
    DateTime CreatedUtc { get; }
}