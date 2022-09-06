using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionTemplateCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateSubmissionTemplateCheckpointByIdCommand, SubmissionTemplateEntity, SubmissionTemplateRecord>
{
    public UpdateSubmissionTemplateCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateSubmissionTemplateCheckpointByIdCommandHandler> logger) : base(eventRepository, submissionTemplateCheckpointRepository, mapper, validator, logger)
    {
    }
}