using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Product;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveProductCampaignsByStoreIdQueryHandler : IRequestHandler<GetActiveProductCampaignsByStoreIdQuery,
    Result<List<CampaignProductPerStoreViewRecord>>>
{
    private CampaignProductSubmissionFieldConfiguration _campaignProductSubmissionFieldConfiguration;

    private readonly ICheckpointRepository<CampaignEntity> _checkpointRepository;
    private readonly ICheckpointRepository<SubmissionEntity> _submissionChekpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _submissionWithStoreContainerRepository;
    private readonly IContainerRepository<ProductViewRecord> _productContainerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetActiveProductCampaignsByStoreIdQuery> _validator;
    private readonly ILogger<GetActiveProductCampaignsByStoreIdQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetActiveProductCampaignsByStoreIdQueryHandler(
        IOptions<CampaignProductSubmissionFieldConfiguration> campaignProductSubmissionFieldConfiguration,
        ICheckpointRepository<CampaignEntity> checkpointRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> submissionWithStoreContainerRepository,
        ICheckpointRepository<SubmissionEntity> submissionChekpointRepository,
        IContainerRepository<ProductViewRecord> productContainerRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetActiveProductCampaignsByStoreIdQuery> validator,
        ILogger<GetActiveProductCampaignsByStoreIdQueryHandler> logger)
    {
        _campaignProductSubmissionFieldConfiguration = campaignProductSubmissionFieldConfiguration.Value;

        _checkpointRepository = checkpointRepository;
        _submissionWithStoreContainerRepository = submissionWithStoreContainerRepository;
        _submissionChekpointRepository = submissionChekpointRepository;
        _productContainerRepository = productContainerRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
        _userService = userService;
    }

    public async Task<Result<List<CampaignProductPerStoreViewRecord>>> Handle(
        GetActiveProductCampaignsByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all campaigns failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CampaignProductPerStoreViewRecord>>.Error(errorMessage);
        }

        if (request.StoreIds is not { Count: > 0 } && request.CampaignIds is not { Count: > 0 })
            return Result<List<CampaignProductPerStoreViewRecord>>.Success(
                new List<CampaignProductPerStoreViewRecord>(0));

        var result = default(Result<List<CampaignProductPerStoreViewRecord>>);

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
                $" AND c.campaignFocusProductIds != null" +
                $" AND ARRAY_LENGTH(c.campaignFocusProductIds) > 0" +
                $" AND c.campaignPublishedDate <= '{utcNow:O}'" +
                $" AND c.campaignEndDate > '{utcNow:O}'";

            if (!string.IsNullOrEmpty(storeIdsString))
                query += $" AND (c.storeIds = null OR ARRAY_LENGTH(c.storeIds) = 0 OR ARRAY_CONTAINS(c.storeIds, {storeIdsString}))";
            if (!string.IsNullOrEmpty(campaignIdsString))
                query += $" AND c.id in ({campaignIdsString})";

            var entities = await _checkpointRepository.QueryAsync(query);
            var campaigns = _mapper.Map<List<CampaignRecord>>(entities);

            // To get product names
            var productIds = campaigns.SelectMany(i => i.CampaignFocusProductIds!).Distinct().ToList();
            var productsTask = GetProductsByIds(productIds);

            var productSubmissionsTask =
                GetCampaignProductSubmissions(
                    campaigns.Select(s => s.Id).ToList(),
                    request.StoreIds ?? new List<Guid>(0));

            await Task.WhenAll(productsTask, productSubmissionsTask);

            var productsLookup = (await productsTask).ToDictionary(i => i.Id, i => i);
            var productSubmissionsLookup = await productSubmissionsTask;

            var currentUserId = Guid.Parse(_userService.CurrentUserId());
            var activeCampaigns = new List<CampaignProductPerStoreViewRecord>();

            foreach (var campaign in campaigns)
            {
                foreach (var storeId in campaign.StoreIds!)
                {
                    foreach (var productId in
                             campaign.CampaignFocusProductIds!.Where(i => productsLookup.ContainsKey(i)))
                    {
                        var key = (campaign.Id, storeId, productId);
                        productSubmissionsLookup.TryGetValue(key, out var submissions);
                        submissions ??= new List<CampaignProductSubmissionViewRecord>(0);

                        // Exclude campaigns with required submissions
                        // Or if have been completed by current user
                        if (submissions.Count < campaign.RequiredSubmissions &&
                            submissions.All(i => i.UserId != currentUserId))
                        {
                            var campaignToBeAdded = new CampaignProductPerStoreViewRecord()
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
                                FocusProductId = productId,
                                FocusProductName = productsLookup[productId].Name,
                                CampaignProductSubmissions = submissions
                            };

                            activeCampaigns.Add(campaignToBeAdded);
                        }
                    }
                }
            }

            return Result<List<CampaignProductPerStoreViewRecord>>.Success(activeCampaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CampaignProductPerStoreViewRecord>>.Error(ex);
        }

        return result;
    }

    private async
        Task<Dictionary<(Guid campaignId, Guid storeId, Guid productId), List<CampaignProductSubmissionViewRecord>>>
        GetCampaignProductSubmissions(List<Guid> campaignIds, List<Guid> storeIds)
    {
        // Get related campaign submissions
        var campaignSubmissions = await GetSubmissionsWithStore(campaignIds, storeIds);
        var fullSubmissions = await GetSubmissionsWithResults(campaignSubmissions.Select(i => i.Id).ToList());

        var submissionLookup =
            new Dictionary<(Guid campaignId, Guid storeId, Guid productId),
                List<CampaignProductSubmissionViewRecord>>();

        // Process campaign submissions
        foreach (var camSub in campaignSubmissions)
        {
            if (fullSubmissions.FirstOrDefault(i => i.Id == camSub.Id) is { } fullSub)
            {
                var focusProduct = fullSub.GetFirstResultByTemplateFieldId<ItemValueRecord>(
                    _campaignProductSubmissionFieldConfiguration.ProductFocusFieldId, SubmissionFieldType.focus);

                if (Guid.TryParse(focusProduct?.ItemId, out var productId))
                {
                    var proSub = new CampaignProductSubmissionViewRecord()
                    {
                        CampaignId = camSub.CampaignId!.Value,
                        StoreId = camSub.StoreId,
                        StoreName = camSub.StoreName ?? string.Empty,
                        UserId = camSub.UserId,
                        SubmissionId = camSub.Id,
                        FocusProductId = productId,
                        FocusProductName = focusProduct.ItemName,
                        Aisle =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignProductSubmissionFieldConfiguration.AisleFieldId,
                                SubmissionFieldType.picker),
                        Section =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignProductSubmissionFieldConfiguration.ShelfSectionFieldId,
                                SubmissionFieldType.picker),
                        Shelf =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignProductSubmissionFieldConfiguration.ShelfLabelFieldId,
                                SubmissionFieldType.picker),
                        Price =
                            fullSub.GetFirstResultByTemplateFieldId<double?>(
                                _campaignProductSubmissionFieldConfiguration.ProductPriceFieldId,
                                SubmissionFieldType.currency),
                        Quantity =
                            fullSub.GetFirstResultByTemplateFieldId<int?>(
                                _campaignProductSubmissionFieldConfiguration.QuantityFieldId,
                                SubmissionFieldType.integer),
                        NearestUseByDate =
                            fullSub.GetFirstResultByTemplateFieldId<DateTime?>(
                                _campaignProductSubmissionFieldConfiguration.NearestUseByDateFieldId,
                                SubmissionFieldType.date),
                        Issue =
                            fullSub.GetFirstResultByTemplateFieldId<string>(
                                _campaignProductSubmissionFieldConfiguration.ShelfIssueFieldId,
                                SubmissionFieldType.picker),
                        Comments = fullSub
                            .GetFirstResultByTemplateFieldId<string>(
                                _campaignProductSubmissionFieldConfiguration.CommentsFieldId,
                                SubmissionFieldType.text),
                    };

                    var key = (proSub.CampaignId, proSub.StoreId, proSub.FocusProductId);
                    if (!submissionLookup.ContainsKey(key))
                    {
                        submissionLookup[key] = new List<CampaignProductSubmissionViewRecord>();
                    }

                    submissionLookup[key].Add(proSub);
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

        var result = await _submissionChekpointRepository.QueryAsync(sqlQueryText);
        return result;
    }

    private async Task<List<ProductViewRecord>> GetProductsByIds(List<Guid> productIds)
    {
        if (!productIds.Any())
            return new List<ProductViewRecord>(0);

        var productIdString = string.Join(',', productIds.Select(s => $"'{s}'"));
        var sqlQueryText = $"SELECT * FROM c WHERE c.id IN ({productIdString})";

        var result = await _productContainerRepository.QueryAsync(sqlQueryText);
        return result;
    }
}