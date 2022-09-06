using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateSubmissionCheckpointByIdCommand, SubmissionEntity, SubmissionRecord>
{
    public UpdateSubmissionCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateSubmissionCheckpointByIdCommandHandler> logger) : base(eventRepository, submissionCheckpointRepository, mapper, validator, logger)
    {
    }
}