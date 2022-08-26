using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildSubmissionCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildSubmissionCheckpointCommand, SubmissionTemplateEntity>
{
    public RebuildSubmissionCheckpointCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionCheckpointRepository,
        ILogger<RebuildSubmissionCheckpointCommandHandler> logger) : base(submissionCheckpointRepository, logger)
    {
    }
}