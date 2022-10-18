using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Campaign.Application.Commands;

public class UpdateCampaignViewCommandHandler : IRequestHandler<UpdateCampaignViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CampaignEntity> _campaignCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<CampaignViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateCampaignViewCommandHandler> _logger;

    public UpdateCampaignViewCommandHandler(
        ICheckpointRepository<CampaignEntity> campaignCheckpointRepository,
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<CampaignViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateCampaignViewCommandHandler> logger) : base()
    {
        _campaignCheckpointRepository = campaignCheckpointRepository;
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateCampaignViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.CampaignId.HasValue && !command.SubmissionId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            Guid? campaignId = command.CampaignId;
            if (command.SubmissionId.HasValue)
            {

                var submissionEntity = await _submissionCheckpointRepository.GetByIdAsync(command.SubmissionId.Value);
                if (submissionEntity != null)
                {
                    campaignId = submissionEntity.CampaignId;
                }
            }

            if (!campaignId.HasValue)
                return Result<int>.Success(0);

            var campaignTask = _campaignCheckpointRepository.GetByIdAsync(campaignId.Value);
            var affectedSubmissionsTask = GetSubmissionsByCampaignIdAsync(campaignId.Value);

            await Task.WhenAll(campaignTask, affectedSubmissionsTask);

            try
            {
                if (campaignTask.Result != null)
                {
                    var campaign = campaignTask.Result;
                    var completions = 0;
                    if (affectedSubmissionsTask.Result != null)
                    {
                        completions = affectedSubmissionsTask.Result.Count();
                    }

                    var templates = await GetSubmissionTemplatesAsync(campaign.SelectedTemplateIds);
                    var templateTitles = string.Empty;

                    if (templates != null)
                    {
                        templateTitles = string.Join(',', templates.Select(t => t.Title));
                    }

                    var campaignView = new CampaignViewRecord(
                        campaign.Id,
                        campaign.Name,
                        campaign.CampaignType,
                        templateTitles,
                        campaign.StoreIds?.Count ?? 0,
                        completions,
                        campaign.CampaignPublishedDate,
                        campaign.CampaignStatus
                        );

                    var campaignResult = await _containerRepository.UpsertAsync(campaignView);
                    return Result<int>.Success(campaignResult != null ? 1 : 0);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update campaignId view for '{command.CampaignId}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }

    private async Task<List<SubmissionEntity>> GetSubmissionsByCampaignIdAsync(Guid? campaignId)
    {
        const string campaignIdKey = "@campaignId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (campaignId.HasValue)
        {
            conditions.Add($"c.campaignId = {campaignIdKey}");
            parameters.Add(campaignIdKey, campaignId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<SubmissionEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedSubmissions = await _submissionCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSubmissions;
    }

    private async Task<List<SubmissionTemplateEntity>> GetSubmissionTemplatesAsync(List<Guid>? submissionTemplateIds)
    {
        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (submissionTemplateIds != null && submissionTemplateIds.Count > 0)
        {
            conditions.Add($"c.id in ({string.Join(",", submissionTemplateIds.Select(s => $"'{s}'").ToList())})");
        }

        if (!conditions.Any())
            return new List<SubmissionTemplateEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedSubmissionTemplates = await _submissionTemplateCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSubmissionTemplates;
    }


    private static string FailedToMessage(UpdateCampaignViewCommand command) =>
        $"Failed to update submissionTemplate view '{JsonConvert.SerializeObject(command)}'";
}