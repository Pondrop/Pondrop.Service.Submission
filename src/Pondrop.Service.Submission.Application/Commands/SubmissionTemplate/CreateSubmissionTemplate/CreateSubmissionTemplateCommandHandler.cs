using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;

public class CreateSubmissionTemplateCommandHandler : DirtyCommandHandler<SubmissionTemplateEntity, CreateSubmissionTemplateCommand, Result<SubmissionTemplateRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateSubmissionTemplateCommand> _validator;
    private readonly ILogger<CreateSubmissionTemplateCommandHandler> _logger;

    public CreateSubmissionTemplateCommandHandler(
        IOptions<SubmissionUpdateConfiguration> SubmissionTemplateUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateSubmissionTemplateCommand> validator,
        ILogger<CreateSubmissionTemplateCommandHandler> logger) : base(eventRepository, SubmissionTemplateUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SubmissionTemplateRecord>> Handle(CreateSubmissionTemplateCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create submissionTemplate template failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionTemplateRecord>.Error(errorMessage);
        }

        var result = default(Result<SubmissionTemplateRecord>);

        try
        {

            var SubmissionTemplateEntity = new SubmissionTemplateEntity(
                command.Title,
                command.Description,
                command.IconCodePoint,
                command.IconFontFamily,
                command.Type,
                command.Status,
                command.IsForManualSubmissions,
                command.Focus,
                command.InitiatedBy,
                _userService.CurrentUserId()
               );

            foreach (var step in command.Steps)
            {
                SubmissionTemplateEntity.Apply(new AddStepToSubmissionTemplate(
                    Guid.NewGuid(),
                    SubmissionTemplateEntity.Id,
                    step!.Title,
                    step!.Instructions,
                    step!.InstructionsStep,
                    step!.InstructionsContinueButton,
                    step!.InstructionsSkipButton,
                    step!.InstructionsIconCodePoint,
                    step!.InstructionsIconFontFamily,
                    step!.IsSummary,
                    step!.FieldDefinitions,
                    _userService.CurrentUserId(),
                    _userService.CurrentUserId()), _userService.CurrentUserId());
            }
            var success = await _eventRepository.AppendEventsAsync(SubmissionTemplateEntity.StreamId, 0, SubmissionTemplateEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(SubmissionTemplateEntity.Id, SubmissionTemplateEntity.GetEvents()));

            result = success
                ? Result<SubmissionTemplateRecord>.Success(_mapper.Map<SubmissionTemplateRecord>(SubmissionTemplateEntity))
                : Result<SubmissionTemplateRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SubmissionTemplateRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateSubmissionTemplateCommand command) =>
        $"Failed to create submissionTemplate template \nCommand: '{JsonConvert.SerializeObject(command)}'";
}