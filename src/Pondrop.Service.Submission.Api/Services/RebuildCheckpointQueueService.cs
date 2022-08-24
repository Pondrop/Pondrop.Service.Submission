using Pondrop.Service.Submission.Application.Commands;

namespace Pondrop.Service.Submission.Api.Services;

public class RebuildCheckpointQueueService : BaseBackgroundQueueService<RebuildCheckpointCommand>, IRebuildCheckpointQueueService
{
}