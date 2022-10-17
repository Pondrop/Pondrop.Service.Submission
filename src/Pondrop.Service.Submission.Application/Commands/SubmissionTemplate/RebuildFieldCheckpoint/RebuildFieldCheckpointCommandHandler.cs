using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildFieldCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildSubmissionCheckpointCommand, FieldEntity>
{
    public RebuildFieldCheckpointCommandHandler(
        ICheckpointRepository<FieldEntity> submissionTemplateCheckpointRepository,
        ILogger<RebuildSubmissionCheckpointCommandHandler> logger) : base(submissionTemplateCheckpointRepository, logger)
    {
    }
}