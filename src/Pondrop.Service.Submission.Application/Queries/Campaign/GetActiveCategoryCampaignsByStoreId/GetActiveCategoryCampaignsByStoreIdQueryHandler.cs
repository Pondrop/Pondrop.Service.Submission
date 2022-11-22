using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveCategoryCampaignsByStoreIdQueryHandler : IRequestHandler<GetActiveCategoryCampaignsByStoreIdQuery, Result<List<CampaignCategoryPerStoreViewRecord>>>
{
    private readonly ICheckpointRepository<CampaignEntity> _checkpointRepository;
    private readonly ICheckpointRepository<SubmissionEntity> _submissionChekpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _submissionWithStoreContainerRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetActiveCategoryCampaignsByStoreIdQuery> _validator;
    private readonly ILogger<GetActiveCategoryCampaignsByStoreIdQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetActiveCategoryCampaignsByStoreIdQueryHandler(
        ICheckpointRepository<CampaignEntity> checkpointRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> submissionWithStoreContainerRepository,
        ICheckpointRepository<SubmissionEntity> submissionChekpointRepository,
        ICheckpointRepository<CategoryEntity> categoryContainerRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetActiveCategoryCampaignsByStoreIdQuery> validator,
        ILogger<GetActiveCategoryCampaignsByStoreIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _submissionWithStoreContainerRepository = submissionWithStoreContainerRepository;
        _categoryCheckpointRepository = categoryContainerRepository;
        _submissionChekpointRepository = submissionChekpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
        _userService = userService;
    }

    public async Task<Result<List<CampaignCategoryPerStoreViewRecord>>> Handle(GetActiveCategoryCampaignsByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all campaigns failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CampaignCategoryPerStoreViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<CampaignCategoryPerStoreViewRecord>>);

        if ((request.StoreIds == null || request.StoreIds.Count <= 0) && (request.CampaignIds == null || request.CampaignIds.Count <= 0))
            return result;

        try
        {
            var storeIdsString = request.StoreIds != null ? string.Join(',', request.StoreIds.Select(s => $"'{s}'")) : string.Empty;
            var categoryIdsString = request.CampaignIds != null ? string.Join(',', request.CampaignIds.Select(s => $"'{s}'")) : string.Empty;
            var query = $"SELECT * FROM c WHERE c.campaignStatus = 'live' AND c.campaignFocusCategoryIds != null";
            if (!string.IsNullOrEmpty(storeIdsString))
                query += $" AND (ARRAY_CONTAINS(c.storeIds, {storeIdsString}) OR c.storeIds = null)";
            if (!string.IsNullOrEmpty(categoryIdsString))
                query += $" AND c.id in ({categoryIdsString})";

            var entities = await _checkpointRepository.QueryAsync(query);
            var campaigns = _mapper.Map<List<CampaignRecord>>(entities.Where(s => s.CampaignEndDate > DateTime.UtcNow));

            var activeCampaigns = new List<CampaignCategoryPerStoreViewRecord>();
            var submissionsFromAllCampaigns = await GetSubmissionsByCampaignIds(campaigns.Select(s => s.Id).ToList());

            foreach (var campaign in campaigns)
            {
                if (campaign != null && campaign.CampaignFocusCategoryIds != null)
                {
                    var submissions = submissionsFromAllCampaigns.Where(s => s.CampaignId == campaign.Id);

                    if (submissions.Any(s => s.UserId?.ToString() == _userService.CurrentUserId()))
                        continue;

                    var categories = await GetCategoriesByIds(campaign.CampaignFocusCategoryIds);

                    foreach (var focusCategory in campaign.CampaignFocusCategoryIds)
                    {
                        if (submissions == null || campaign.RequiredSubmissions > submissions.Count())
                        {
                            var category = categories.FirstOrDefault(s => s.Id == focusCategory);
                            var categorySubmissions = new List<CampaignCategorySubmissionViewRecord>();

                            if (submissions != null)
                            {

                                foreach (var submission in submissions)
                                {
                                    var submissionFull = await _submissionChekpointRepository.GetByIdAsync(submission.Id);
                                    var products = new List<ItemValueRecord>();

                                    if (submissionFull != null)
                                    {
                                        Guid productListFieldGuid = Guid.Parse("3995d781-e3c4-4407-a1ac-fe613b5c487d");
                                        Guid productFieldGuid = Guid.Parse("2ec0bcdf-340e-4876-89f3-799e6f00e7bb");

                                        foreach (var step in submissionFull.Steps)
                                        {
                                            foreach (var field in step.Fields.Where(f => f?.TemplateFieldId == productFieldGuid || f?.TemplateFieldId == productListFieldGuid))
                                            {
                                                foreach (var itemValue in field?.Values)
                                                {
                                                    products.Add(itemValue?.ItemValue ?? new ItemValueRecord());
                                                }
                                            }
                                        }
                                    }

                                    categorySubmissions.Add(new CampaignCategorySubmissionViewRecord()
                                    {
                                        CampaignId = campaign.Id,
                                        StoreId = submission.StoreId,
                                        StoreName = submission.StoreName ?? string.Empty,
                                        UserId = submission.UserId,
                                        SubmissionId = submission.Id,
                                        FocusCategoryId = focusCategory,
                                        FocusCategoryName = category?.Name ?? string.Empty,
                                        Products = products
                                    });
                                }
                            }

                            var campaignToBeAdded = new CampaignCategoryPerStoreViewRecord()
                            {
                                Id = campaign.Id,
                                Name = campaign.Name,
                                CampaignStatus = campaign.CampaignStatus,
                                StoreId = campaign?.StoreIds?.FirstOrDefault() ?? null,
                                RequiredSubmissions = campaign?.RequiredSubmissions ?? 0,
                                SubmissionTemplateId = campaign?.SelectedTemplateIds?.FirstOrDefault() ?? null,
                                SubmissionCount = submissions?.Count() ?? 0,
                                CampaignEndDate = campaign.CampaignEndDate,
                                CampaignPublishedDate = campaign.CampaignPublishedDate,
                                CampaignType = campaign.CampaignType,
                                FocusCategoryId = focusCategory,
                                FocusCategoryName = category?.Name ?? string.Empty,
                                CampaignCategorySubmissions = categorySubmissions
                            };

                            activeCampaigns.Add(campaignToBeAdded);
                        }
                    }
                }
            }

            return Result<List<CampaignCategoryPerStoreViewRecord>>.Success(activeCampaigns);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CampaignCategoryPerStoreViewRecord>>.Error(ex);
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
    private async Task<List<CategoryEntity>> GetCategoriesByIds(List<Guid>? categoryIds)
    {
        const string categoryIdKey = "@categoryId";

        var conditions = new List<string>();
        //var parameters = new Dictionary<string, string>();

        if (categoryIds != null && categoryIds.Count > 0)
        {
            var categoryIdString = string.Join(',', categoryIds.Select(s => $"'{s}'"));
            conditions.Add($"c.id IN ({categoryIdString})");
        }

        if (!conditions.Any())
            return new List<CategoryEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affected = await _categoryCheckpointRepository.QueryAsync(sqlQueryText);
        return affected;
    }
}