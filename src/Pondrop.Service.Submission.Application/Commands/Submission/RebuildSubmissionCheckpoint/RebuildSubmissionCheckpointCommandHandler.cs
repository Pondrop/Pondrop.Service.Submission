using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildSubmissionCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildSubmissionCheckpointCommand, SubmissionEntity>
{
    public RebuildSubmissionCheckpointCommandHandler(
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ILogger<RebuildSubmissionCheckpointCommandHandler> logger) : base(submissionCheckpointRepository, logger)
    {
    }
}