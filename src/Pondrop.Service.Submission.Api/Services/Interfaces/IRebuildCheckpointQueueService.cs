using Pondrop.Service.Submission.Application.Commands;

namespace Pondrop.Service.Submission.Api.Services;

public interface IRebuildCheckpointQueueService
{
    Task<RebuildCheckpointCommand> DequeueAsync(CancellationToken cancellationToken);
    void Queue(RebuildCheckpointCommand command);
}