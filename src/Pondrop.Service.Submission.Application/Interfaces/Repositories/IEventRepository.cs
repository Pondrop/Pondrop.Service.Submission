using Pondrop.Service.Submission.Domain.Events;

namespace Pondrop.Service.Submission.Application.Interfaces;

public interface IEventRepository
{
    Task<bool> IsConnectedAsync();
    Task<bool> AppendEventsAsync(string streamId, long expectedVersion, IEnumerable<IEvent> events);
    Task<EventStream> LoadStreamAsync(string streamId);
    Task<EventStream> LoadStreamAsync(string streamId, long fromSequenceNumber);
    Task<Dictionary<string, EventStream>> LoadStreamsByTypeAsync(string streamType);
    Task<Dictionary<string, EventStream>> LoadStreamsByTypeAsync(string streamType, DateTime fromDateTime);
}