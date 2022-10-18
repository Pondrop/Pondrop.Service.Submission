using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.Campaign;
using Pondrop.Service.Submission.Domain.Events.Campaign;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Campaign.Application.Commands;

public class UpdateCampaignCommandHandler : DirtyCommandHandler<CampaignEntity, UpdateCampaignCommand, Result<CampaignRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<CampaignEntity> _campaignCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateCampaignCommand> _validator;
    private readonly ILogger<UpdateCampaignCommandHandler> _logger;

    public UpdateCampaignCommandHandler(
        IOptions<SubmissionUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<CampaignEntity> campaignCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateCampaignCommand> validator,
        ILogger<UpdateCampaignCommandHandler> logger) : base(eventRepository, storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _campaignCheckpointRepository = campaignCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<CampaignRecord>> Handle(UpdateCampaignCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<CampaignRecord>.Error(errorMessage);
        }

        var result = default(Result<CampaignRecord>);

        try
        {
            var storeEntity = await _campaignCheckpointRepository.GetByIdAsync(command.Id);
            storeEntity ??= await GetFromStreamAsync(command.Id);

            if (storeEntity is not null)
            {
                var evtPayload = new UpdateCampaign(
                    command.Id,
                command.Name,
                command.CampaignType,
                command.CampaignTriggerIds,
                command.CampaignFocusCategoryIds,
                command.CampaignFocusProductIds,
                command.SelectedTemplateIds,
                command.StoreIds,
                command.RequiredSubmissions,
                command.RewardSchemeId,
                command.CampaignPublishedDate,
                command.CampaignEndDate,
                command.CampaignStatus,
                command.PublicationlifecycleId
               );

                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(storeEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _campaignCheckpointRepository.FastForwardAsync(storeEntity);
                    success = await UpdateStreamAsync(storeEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(storeEntity.Id, storeEntity.GetEvents(storeEntity.AtSequence)));

                result = success
                    ? Result<CampaignRecord>.Success(_mapper.Map<CampaignRecord>(storeEntity))
                    : Result<CampaignRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<CampaignRecord>.Error($"Campaign does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<CampaignRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateCampaignCommand command) =>
        $"Failed to update campaign\nCommand: '{JsonConvert.SerializeObject(command)}'";
}