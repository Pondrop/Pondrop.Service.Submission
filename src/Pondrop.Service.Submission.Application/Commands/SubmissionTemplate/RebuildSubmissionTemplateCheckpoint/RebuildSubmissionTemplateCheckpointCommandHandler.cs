using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildSubmissionTemplateCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildSubmissionCheckpointCommand, SubmissionTemplateEntity>
{
    public RebuildSubmissionTemplateCheckpointCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        ILogger<RebuildSubmissionCheckpointCommandHandler> logger) : base(submissionTemplateCheckpointRepository, logger)
    {
    }
}