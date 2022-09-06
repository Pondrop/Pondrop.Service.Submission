using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events.Submission;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;

public class CreateSubmissionCommandHandler : DirtyCommandHandler<SubmissionEntity, CreateSubmissionCommand, Result<SubmissionRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateSubmissionCommand> _validator;
    private readonly ILogger<CreateSubmissionCommandHandler> _logger;

    public CreateSubmissionCommandHandler(
        IOptions<SubmissionUpdateConfiguration> SubmissionUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateSubmissionCommand> validator,
        ILogger<CreateSubmissionCommandHandler> logger) : base(eventRepository, SubmissionUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SubmissionRecord>> Handle(CreateSubmissionCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create submission template failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionRecord>.Error(errorMessage);
        }

        var result = default(Result<SubmissionRecord>);

        var createdBy = _userService.CurrentUserName();

        try
        {

            var SubmissionEntity = new SubmissionEntity(
                command.StoreVisitId,
                command.SubmissionTemplateId,
                command.Latitude,
                command.Longitude,
                createdBy
               );

            foreach (var step in command.Steps)
            {
                SubmissionEntity.Apply(new Domain.Events.Submission.AddStepToSubmission(
                    Guid.NewGuid(),
                    step!.TemplateStepId,
                    step!.Latitude,
                    step!.Longitude,
                    step!.Fields), createdBy);
            }
            var success = await _eventRepository.AppendEventsAsync(SubmissionEntity.StreamId, 0, SubmissionEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(SubmissionEntity.Id, SubmissionEntity.GetEvents()));

            result = success
                ? Result<SubmissionRecord>.Success(_mapper.Map<SubmissionRecord>(SubmissionEntity))
                : Result<SubmissionRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SubmissionRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateSubmissionCommand command) =>
        $"Failed to create submission template \nCommand: '{JsonConvert.SerializeObject(command)}'";
}