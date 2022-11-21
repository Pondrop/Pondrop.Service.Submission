using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveCampaignsByStoreIdQueryHandler : IRequestHandler<GetActiveCampaignsByStoreIdQuery, Result<List<CampaignRecord>>>
{
    private readonly ICheckpointRepository<CampaignEntity> _checkpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _submissionWithStoreContainerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetActiveCampaignsByStoreIdQuery> _validator;
    private readonly ILogger<GetActiveCampaignsByStoreIdQueryHandler> _logger;

    public GetActiveCampaignsByStoreIdQueryHandler(
        ICheckpointRepository<CampaignEntity> checkpointRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> submissionWithStoreContainerRepository,
        IMapper mapper,
        IValidator<GetActiveCampaignsByStoreIdQuery> validator,
        ILogger<GetActiveCampaignsByStoreIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _submissionWithStoreContainerRepository = submissionWithStoreContainerRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<CampaignRecord>>> Handle(GetActiveCampaignsByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all campaigns failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CampaignRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<CampaignRecord>>);

        if (request.StoreIds == null || request.StoreIds.Count <= 0) 
            return result;

        try
        {
            var storeIdsString = string.Join(',', request.StoreIds.Select(s => $"'{s}'"));
            var query = $"SELECT * FROM c WHERE c.campaignStatus = 'live' AND ARRAY_CONTAINS(c.storeIds, {storeIdsString})";
            var entities = await _checkpointRepository.QueryAsync(query);
            var campaigns = _mapper.Map<List<CampaignRecord>>(entities.Where(s => s.CampaignEndDate > DateTime.UtcNow));

            var submissionsFromAllCampaigns = await GetSubmissionsByCampaignIds(campaigns.Select(s => s.Id).ToList());

            var activeCampaigns = new List<CampaignRecord>();

            foreach (var campaign in campaigns)
            {
                var submissions = submissionsFromAllCampaigns.Where(s => s.CampaignId == campaign.Id);
                if (submissions == null || campaign.RequiredSubmissions > submissions.Count())
                {
                    activeCampaigns.Add(campaign);
                }
            }

            return Result<List<CampaignRecord>>.Success(activeCampaigns);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CampaignRecord>>.Error(ex);
        }

        return result;
    }


    private async Task<List<SubmissionWithStoreViewRecord>> GetSubmissionsByCampaignIds(List<Guid>? campaignIds)
    {
        const string campaignIdKey = "@campaignId";

        var conditions = new List<string>();
        //var parameters = new Dictionary<string, string>();

        if (campaignIds != null && campaignIds.Count > 0)
        {
            var campaignIdsString = string.Join(',', campaignIds.Select(s => $"'{s}'"));
            conditions.Add($"c.campaignId IN ({campaignIdsString})");
        }


        if (!conditions.Any())
            return new List<SubmissionWithStoreViewRecord>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affected = await _submissionWithStoreContainerRepository.QueryAsync(sqlQueryText);
        return affected;
    }
}