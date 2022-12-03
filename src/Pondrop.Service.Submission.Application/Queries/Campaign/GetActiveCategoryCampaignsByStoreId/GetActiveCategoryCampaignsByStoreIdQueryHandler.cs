﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveCategoryCampaignsByStoreIdQueryHandler : IRequestHandler<GetActiveCategoryCampaignsByStoreIdQuery,
    Result<List<CampaignCategoryPerStoreViewRecord>>>
{
    private CampaignCategorySubmissionFieldConfiguration _campaignCategorySubmissionFieldConfiguration;

    private readonly ICheckpointRepository<CampaignEntity> _checkpointRepository;
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _submissionWithStoreContainerRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetActiveCategoryCampaignsByStoreIdQuery> _validator;
    private readonly ILogger<GetActiveCategoryCampaignsByStoreIdQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetActiveCategoryCampaignsByStoreIdQueryHandler(
        IOptions<CampaignCategorySubmissionFieldConfiguration> campaignCategorySubmissionFieldConfiguration,
        ICheckpointRepository<CampaignEntity> checkpointRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> submissionWithStoreContainerRepository,
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetActiveCategoryCampaignsByStoreIdQuery> validator,
        ILogger<GetActiveCategoryCampaignsByStoreIdQueryHandler> logger)
    {
        _campaignCategorySubmissionFieldConfiguration = campaignCategorySubmissionFieldConfiguration.Value;

        _checkpointRepository = checkpointRepository;
        _submissionWithStoreContainerRepository = submissionWithStoreContainerRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
        _userService = userService;
    }

    public async Task<Result<List<CampaignCategoryPerStoreViewRecord>>> Handle(
        GetActiveCategoryCampaignsByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all campaigns failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CampaignCategoryPerStoreViewRecord>>.Error(errorMessage);
        }

        if (request.StoreIds is not { Count: > 0 } && request.CampaignIds is not { Count: > 0 })
            return Result<List<CampaignCategoryPerStoreViewRecord>>.Success(
                new List<CampaignCategoryPerStoreViewRecord>(0));

        var result = default(Result<List<CampaignCategoryPerStoreViewRecord>>);

        try
        {
            var storeIdsString = request.StoreIds?.Any() == true
                ? string.Join(',', request.StoreIds.Select(s => $"'{s}'"))
                : string.Empty;
            var campaignIdsString = request.CampaignIds?.Any() == true
                ? string.Join(',', request.CampaignIds.Select(s => $"'{s}'"))
                : string.Empty;

            var utcNow = DateTime.UtcNow;

            var query =
                $"SELECT * FROM c" +
                $" WHERE c.campaignStatus = 'live'" +
                $" AND c.campaignFocusCategoryIds != null" +
                $" AND ARRAY_LENGTH(c.campaignFocusCategoryIds) > 0" +
                $" AND c.campaignPublishedDate <= '{utcNow:O}'" +
                $" AND c.campaignEndDate > '{utcNow:O}'";

            if (!string.IsNullOrEmpty(storeIdsString))
                query +=
                    $" AND (c.storeIds = null OR ARRAY_LENGTH(c.storeIds) = 0 OR ARRAY_CONTAINS(c.storeIds, {storeIdsString}))";
            if (!string.IsNullOrEmpty(campaignIdsString))
                query += $" AND c.id in ({campaignIdsString})";

            var entities = await _checkpointRepository.QueryAsync(query);
            var campaigns = _mapper.Map<List<CampaignRecord>>(entities);

            // To get category names
            var categoryIds = campaigns.SelectMany(i => i.CampaignFocusCategoryIds!).Distinct().ToList();
            var categoriesTask = GetCategoriesByIds(categoryIds);

            var categorySubmissionsTask =
                GetCampaignCategorySubmissions(
                    campaigns.Select(s => s.Id).ToList(),
                    request.StoreIds ?? new List<Guid>(0));

            await Task.WhenAll(categoriesTask, categorySubmissionsTask);

            var categoriesLookup = (await categoriesTask).ToDictionary(i => i.Id, i => i);
            var categorySubmissionsLookup = await categorySubmissionsTask;

            var currentUserId = Guid.Parse(_userService.CurrentUserId());
            var activeCampaigns = new List<CampaignCategoryPerStoreViewRecord>();

            foreach (var campaign in campaigns)
            {
                foreach (var storeId in campaign.StoreIds!)
                {
                    foreach (var categoryId in
                             campaign.CampaignFocusCategoryIds!.Where(i => categoriesLookup.ContainsKey(i)))
                    {
                        var key = (campaign.Id, storeId, categoryId);
                        categorySubmissionsLookup.TryGetValue(key, out var submissions);
                        submissions ??= new List<CampaignCategorySubmissionViewRecord>(0);

                        // Exclude campaigns with required submissions
                        // Or if have been completed by current user
                        if (submissions.Count < campaign.RequiredSubmissions &&
                            submissions.All(i => i.UserId != currentUserId))
                        {
                            var campaignToBeAdded = new CampaignCategoryPerStoreViewRecord()
                            {
                                Id = campaign.Id,
                                Name = campaign.Name,
                                CampaignStatus = campaign.CampaignStatus,
                                StoreId = storeId,
                                SubmissionCount = submissions.Count,
                                SubmissionTemplateId = campaign.SelectedTemplateIds?.FirstOrDefault() ?? Guid.Empty,
                                RequiredSubmissions = campaign.RequiredSubmissions,
                                CampaignEndDate = campaign.CampaignEndDate,
                                CampaignPublishedDate = campaign.CampaignPublishedDate,
                                CampaignType = campaign.CampaignType,
                                FocusCategoryId = categoryId,
                                FocusCategoryName = categoriesLookup[categoryId].Name,
                                CampaignCategorySubmissions = submissions
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

    private async
        Task<Dictionary<(Guid campaignId, Guid storeId, Guid categoryId), List<CampaignCategorySubmissionViewRecord>>>
        GetCampaignCategorySubmissions(List<Guid> campaignIds, List<Guid> storeIds)
    {
        // Get related campaign submissions
        var campaignSubmissions = await GetSubmissionsWithStore(campaignIds, storeIds);
        var fullSubmissions = await GetSubmissionsWithResults(campaignSubmissions.Select(i => i.Id).ToList());

        var submissionLookup =
            new Dictionary<(Guid campaignId, Guid storeId, Guid categoryId),
                List<CampaignCategorySubmissionViewRecord>>();

        // Process campaign submissions
        foreach (var camSub in campaignSubmissions)
        {
            if (fullSubmissions.FirstOrDefault(i => i.Id == camSub.Id) is { } fullSub)
            {
                var focusCategory = fullSub.GetFirstResultByTemplateFieldId<ItemValueRecord>(
                    _campaignCategorySubmissionFieldConfiguration.CategoryFocusFieldId, SubmissionFieldType.focus);

                if (Guid.TryParse(focusCategory?.ItemId, out var categoryId))
                {
                    var catSub = new CampaignCategorySubmissionViewRecord()
                    {
                        CampaignId = camSub.CampaignId!.Value,
                        StoreId = camSub.StoreId,
                        StoreName = camSub.StoreName ?? string.Empty,
                        UserId = camSub.UserId,
                        SubmissionId = camSub.Id,
                        FocusCategoryId = categoryId,
                        FocusCategoryName = focusCategory.ItemName,
                        Aisle =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignCategorySubmissionFieldConfiguration.AisleFieldId,
                                SubmissionFieldType.picker),
                        Section =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignCategorySubmissionFieldConfiguration.ShelfSectionFieldId,
                                SubmissionFieldType.picker),
                        Shelf =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignCategorySubmissionFieldConfiguration.ShelfLabelFieldId,
                                SubmissionFieldType.picker),
                        Products =
                            fullSub.GetResultsByTemplateFieldId(
                                    _campaignCategorySubmissionFieldConfiguration.ProductsFieldId,
                                    SubmissionFieldType.search)
                                .OfType<ItemValueRecord>().ToList(),
                        Issue =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignCategorySubmissionFieldConfiguration.ShelfIssueFieldId,
                                SubmissionFieldType.picker),
                        Comments = fullSub
                            .GetFirstResultByTemplateFieldId<string>(
                                _campaignCategorySubmissionFieldConfiguration.CommentsFieldId,
                                SubmissionFieldType.text),
                    };

                    var key = (catSub.CampaignId, catSub.StoreId, catSub.FocusCategoryId);
                    if (!submissionLookup.ContainsKey(key))
                    {
                        submissionLookup[key] = new List<CampaignCategorySubmissionViewRecord>();
                    }

                    submissionLookup[key].Add(catSub);
                }
            }
        }

        return submissionLookup;
    }

    private async Task<List<SubmissionWithStoreViewRecord>> GetSubmissionsWithStore(List<Guid> campaignIds,
        List<Guid> storeIds)
    {
        if (!campaignIds.Any())
            return new List<SubmissionWithStoreViewRecord>(0);

        var campaignIdsString = string.Join(',', campaignIds.Select(s => $"'{s}'"));

        var sqlQueryText = $"SELECT * FROM c WHERE c.campaignId IN ({campaignIdsString})";

        if (storeIds.Any())
        {
            var storeIdsString = string.Join(',', storeIds.Select(s => $"'{s}'"));
            sqlQueryText += $" AND c.storeId IN ({storeIdsString})";
        }

        var result = await _submissionWithStoreContainerRepository.QueryAsync(sqlQueryText);
        return result;
    }

    private async Task<List<SubmissionEntity>> GetSubmissionsWithResults(List<Guid> submissionIds)
    {
        if (!submissionIds.Any())
            return new List<SubmissionEntity>(0);

        var submissionIdsString = string.Join(',', submissionIds.Select(s => $"'{s}'"));
        var sqlQueryText = $"SELECT * FROM c WHERE c.id IN ({submissionIdsString})";

        var result = await _submissionCheckpointRepository.QueryAsync(sqlQueryText);
        return result;
    }

    private async Task<List<CategoryEntity>> GetCategoriesByIds(List<Guid> categoryIds)
    {
        if (!categoryIds.Any())
            return new List<CategoryEntity>(0);

        var categoryIdString = string.Join(',', categoryIds.Select(s => $"'{s}'"));
        var sqlQueryText = $"SELECT * FROM c WHERE c.id IN ({categoryIdString})";

        var result = await _categoryCheckpointRepository.QueryAsync(sqlQueryText);
        return result;
    }
}