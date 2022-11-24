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

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class AddStepToSubmissionTemplateCommandHandler : DirtyCommandHandler<SubmissionTemplateEntity, AddStepToSubmissionTemplateCommand, Result<SubmissionTemplateRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<AddStepToSubmissionTemplateCommand> _validator;
    private readonly ILogger<AddStepToSubmissionTemplateCommandHandler> _logger;

    public AddStepToSubmissionTemplateCommandHandler(
        IOptions<SubmissionUpdateConfiguration> submissionTemplateUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<AddStepToSubmissionTemplateCommand> validator,
        ILogger<AddStepToSubmissionTemplateCommandHandler> logger) : base(eventRepository, submissionTemplateUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SubmissionTemplateRecord>> Handle(AddStepToSubmissionTemplateCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create step template failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionTemplateRecord>.Error(errorMessage);
        }
        
        Result<SubmissionTemplateRecord> result;

        try
        {
            var submissionTemplateEntity = await _submissionTemplateCheckpointRepository.GetByIdAsync(command.SubmissionId);
            submissionTemplateEntity ??= await GetFromStreamAsync(command.SubmissionId);

            if (submissionTemplateEntity is not null)
            {
                var evtPayload = new AddStepToSubmissionTemplate(
                    Guid.NewGuid(),
                    submissionTemplateEntity.Id,
                    command.Title,
                    command!.Instructions,
                    command!.InstructionsStep,
                    command!.InstructionsContinueButton,
                    command!.InstructionsSkipButton,
                    command!.InstructionsIconCodePoint,
                    command!.InstructionsIconFontFamily,
                    command!.IsSummary,
                    command!.FieldIds,
                    _userService.CurrentUserId(),
                    _userService.CurrentUserId());

                var success = await UpdateStreamAsync(submissionTemplateEntity, evtPayload, _userService.CurrentUserId());

                if (!success)
                {
                    await _submissionTemplateCheckpointRepository.FastForwardAsync(submissionTemplateEntity);
                    success = await UpdateStreamAsync(submissionTemplateEntity, evtPayload, _userService.CurrentUserId());
                }

                await Task.WhenAll(
                    InvokeDaprMethods(submissionTemplateEntity.Id, submissionTemplateEntity.GetEvents(submissionTemplateEntity.AtSequence)));

                result = success
                    ? Result<SubmissionTemplateRecord>.Success(_mapper.Map<SubmissionTemplateRecord>(submissionTemplateEntity))
                    : Result<SubmissionTemplateRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<SubmissionTemplateRecord>.Error("Submission does not exist");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SubmissionTemplateRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(AddStepToSubmissionTemplateCommand command) =>
        $"Failed to create submissionTemplate step template\nCommand: '{JsonConvert.SerializeObject(command)}'";
}