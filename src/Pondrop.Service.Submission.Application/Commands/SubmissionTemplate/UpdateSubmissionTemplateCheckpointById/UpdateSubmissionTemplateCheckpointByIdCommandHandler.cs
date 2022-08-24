using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateSubmissionCheckpointByIdCommand, SubmissionTemplateEntity, SubmissionTemplateRecord>
{
    public UpdateSubmissionCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateSubmissionCheckpointByIdCommandHandler> logger) : base(eventRepository, submissionCheckpointRepository, mapper, validator, logger)
    {
    }
}