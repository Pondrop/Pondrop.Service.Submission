using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.Field.AddStepToSubmission;

public class CreateFieldCommandHandler : DirtyCommandHandler<FieldEntity, CreateFieldCommand, Result<FieldRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<FieldEntity> _submissionTemplateCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateFieldCommand> _validator;
    private readonly ILogger<CreateFieldCommandHandler> _logger;

    public CreateFieldCommandHandler(
        IOptions<SubmissionUpdateConfiguration> submissionTemplateUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<FieldEntity> submissionTemplateCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateFieldCommand> validator,
        ILogger<CreateFieldCommandHandler> logger) : base(eventRepository, submissionTemplateUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<FieldRecord>> Handle(CreateFieldCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create step template failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<FieldRecord>.Error(errorMessage);
        }

        Result<FieldRecord> result;

        try
        {
            var fieldEntity = new FieldEntity(
                command.Label,
                command!.Mandatory,
                command!.FieldType,
                command!.ItemType,
                command!.MaxValue,
                command!.PickerValues,
                _userService.CurrentUserId());

            var success = await _eventRepository.AppendEventsAsync(fieldEntity.StreamId, 0, fieldEntity.GetEvents()); ;

            await Task.WhenAll(
                    InvokeDaprMethods(fieldEntity.Id, fieldEntity.GetEvents()));

            result = success
                ? Result<FieldRecord>.Success(_mapper.Map<FieldRecord>(fieldEntity))
                : Result<FieldRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<FieldRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateFieldCommand command) =>
        $"Failed to create field\nCommand: '{JsonConvert.SerializeObject(command)}'";
}