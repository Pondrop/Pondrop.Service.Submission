using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Campaign.Application.Commands;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.SubmissionTemplate.Application.Commands;

public class UpdateSubmissionTemplateCommandHandler : DirtyCommandHandler<SubmissionTemplateEntity, UpdateSubmissionTemplateCommand, Result<SubmissionTemplateRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateSubmissionTemplateCommand> _validator;
    private readonly ILogger<UpdateSubmissionTemplateCommandHandler> _logger;

    public UpdateSubmissionTemplateCommandHandler(
        IOptions<SubmissionUpdateConfiguration> submissionTemplateUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateSubmissionTemplateCommand> validator,
        ILogger<UpdateSubmissionTemplateCommandHandler> logger) : base(eventRepository, submissionTemplateUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SubmissionTemplateRecord>> Handle(UpdateSubmissionTemplateCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update submissionTemplate failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionTemplateRecord>.Error(errorMessage);
        }

        var result = default(Result<SubmissionTemplateRecord>);

        try
        {
            var submissionTemplateEntity = await _submissionTemplateCheckpointRepository.GetByIdAsync(command.Id);
            submissionTemplateEntity ??= await GetFromStreamAsync(command.Id);

            if (submissionTemplateEntity is not null)
            {

                try
                {
                    var evtPayload = new UpdateSubmissionTemplate(
                        command.Id,
                        command.Title,
                        command.Description,
                        command.IconCodePoint,
                        command.IconFontFamily,
                        command.Type,
                        command.Status,
                        command.IsForManualSubmissions,
                        command.Focus,
                        command.InitiatedBy
                       );


                    var createdBy = _userService.CurrentUserId();

                    var success = await UpdateStreamAsync(submissionTemplateEntity, evtPayload, createdBy);

                    if (!success)
                    {
                        await _submissionTemplateCheckpointRepository.FastForwardAsync(submissionTemplateEntity);
                        success = await UpdateStreamAsync(submissionTemplateEntity, evtPayload, createdBy);
                    }

                    await Task.WhenAll(
                        InvokeDaprMethods(submissionTemplateEntity.Id, submissionTemplateEntity.GetEvents(submissionTemplateEntity.AtSequence)));

                    result = success
                        ? Result<SubmissionTemplateRecord>.Success(_mapper.Map<SubmissionTemplateRecord>(submissionTemplateEntity))
                        : Result<SubmissionTemplateRecord>.Error(FailedToCreateMessage(command));

                }
                catch
                {

                }
            }
            else
            {
                result = Result<SubmissionTemplateRecord>.Error($"SubmissionTemplate does not exist '{command.Id}'");
            }
            }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SubmissionTemplateRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateSubmissionTemplateCommand command) =>
        $"Failed to update submissionTemplate\nCommand: '{JsonConvert.SerializeObject(command)}'";
}