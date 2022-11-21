using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
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