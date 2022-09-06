using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;
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