using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildSubmissionTemplateCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildSubmissionTemplateCheckpointCommand, SubmissionTemplateEntity>
{
    public RebuildSubmissionTemplateCheckpointCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        ILogger<RebuildSubmissionTemplateCheckpointCommandHandler> logger) : base(submissionTemplateCheckpointRepository, logger)
    {
    }
}