namespace Pondrop.Service.Submission.Domain.Events;

public interface IEventTypePayloadResolver
{
    Type? GetEventPayloadType(string streamType, string typeName);
}