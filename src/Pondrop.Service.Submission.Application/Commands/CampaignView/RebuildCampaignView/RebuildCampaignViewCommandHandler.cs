using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Campaign.Application.Commands;

public class RebuildCampaignViewCommandHandler : IRequestHandler<RebuildCampaignViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CampaignEntity> _campaignCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<CampaignViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildCampaignViewCommandHandler> _logger;

    public RebuildCampaignViewCommandHandler(
    ICheckpointRepository<CampaignEntity> campaignCheckpointRepository,
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<CampaignViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildCampaignViewCommandHandler> logger) : base()
    {
        _campaignCheckpointRepository = campaignCheckpointRepository;
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildCampaignViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var campaignTask = _campaignCheckpointRepository.GetAllAsync();

            await Task.WhenAll(campaignTask);

            var tasks = campaignTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var affectedSubmissionsTask = GetSubmissionsByCampaignIdAsync(i.Id);

                    await Task.WhenAll(affectedSubmissionsTask);


                    if (campaignTask.Result != null)
                    {
                        var campaign = campaignTask.Result;
                        var completions = 0;
                        if (affectedSubmissionsTask.Result != null)
                        {
                            completions = affectedSubmissionsTask.Result.Count();
                        }

                        var templates = await GetSubmissionTemplatesAsync(i.SelectedTemplateIds);
                        var templateTitles = string.Empty;

                        if (templates != null)
                        {
                            templateTitles = string.Join(',', templates.Select(t => t.Title));
                        }

                        var campaignView = new CampaignViewRecord(
                            i.Id,
                            i.Name,
                            i.CampaignType.HasValue ? i.CampaignType.Value.ToString().FirstCharToUpper() : string.Empty,
                            templateTitles,
                            i.StoreIds?.Count ?? 0,
                            completions,
                            i.CampaignPublishedDate ?? DateTime.MinValue,
                            i.CampaignStartDate ?? DateTime.MinValue,
                            i.CampaignEndDate ?? DateTime.MinValue,
                            i.CampaignStatus.HasValue ? i.CampaignStatus.Value.ToString().FirstCharToUpper() : string.Empty
                            );

                        var campaignResult = await _containerRepository.UpsertAsync(campaignView);
                        success = result != null;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update campaignId view for '{i.Id}'");
                }
                return success;

            }).ToList();

            await Task.WhenAll(tasks);

            result = Result<int>.Success(tasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
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
}

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
}