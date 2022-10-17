using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateFieldCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateFieldCheckpointByIdCommand, FieldEntity, FieldRecord>
{
    public UpdateFieldCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<FieldEntity> submissionTemplateCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateFieldCheckpointByIdCommandHandler> logger) : base(eventRepository, submissionTemplateCheckpointRepository, mapper, validator, logger)
    {
    }
}