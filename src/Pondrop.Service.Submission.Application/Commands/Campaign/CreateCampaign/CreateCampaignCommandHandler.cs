using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateCampaign;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.Campaign;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Campaign.Application.Commands.Campaign.CreateCampaign;

public class CreateCampaignCommandHandler : DirtyCommandHandler<CampaignEntity, CreateCampaignCommand, Result<CampaignRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateCampaignCommand> _validator;
    private readonly ILogger<CreateCampaignCommandHandler> _logger;

    public CreateCampaignCommandHandler(
        IOptions<CampaignUpdateConfiguration> CampaignUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateCampaignCommand> validator,
        ILogger<CreateCampaignCommandHandler> logger) : base(eventRepository, CampaignUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<CampaignRecord>> Handle(CreateCampaignCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create campaign failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<CampaignRecord>.Error(errorMessage);
        }

        var result = default(Result<CampaignRecord>);

        var createdBy = _userService.CurrentUserId();

        try
        {
            var campaignEntity = new CampaignEntity(
                command.Name,
                command.CampaignType,
                command.CampaignTriggerIds,
                command.CampaignFocusCategoryIds,
                command.CampaignFocusProductIds,
                command.SelectedTemplateIds,
                command.StoreIds,
                command.RequiredSubmissions,
                command.RewardSchemeId,
                command.CampaignPublishedDate ?? (command.CampaignStatus == CampaignStatus.live ? DateTime.UtcNow : null),
                command.CampaignEndDate,
                command.CampaignStartDate,
                command.MinimumTimeIntervalMins,
                command.RepeatEvery,
                command.RepeatEveryUOM,
                command.CampaignStatus,
                command.PublicationlifecycleId,
                createdBy
               );

            var success = await _eventRepository.AppendEventsAsync(campaignEntity.StreamId, 0, campaignEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(campaignEntity.Id, campaignEntity.GetEvents()));

            result = success
                ? Result<CampaignRecord>.Success(_mapper.Map<CampaignRecord>(campaignEntity))
                : Result<CampaignRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<CampaignRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateCampaignCommand command) =>
        $"Failed to create campaign \nCommand: '{JsonConvert.SerializeObject(command)}'";
}