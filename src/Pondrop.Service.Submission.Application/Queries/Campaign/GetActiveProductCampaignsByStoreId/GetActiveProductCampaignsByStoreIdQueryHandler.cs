using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Product;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveProductCampaignsByStoreIdQueryHandler : IRequestHandler<GetActiveProductCampaignsByStoreIdQuery, Result<List<CampaignProductPerStoreViewRecord>>>
{
    private readonly ICheckpointRepository<CampaignEntity> _checkpointRepository;
    private readonly ICheckpointRepository<SubmissionEntity> _submissionChekpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _submissionWithStoreContainerRepository;
    private readonly IContainerRepository<ProductViewRecord> _productContainerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetActiveProductCampaignsByStoreIdQuery> _validator;
    private readonly ILogger<GetActiveProductCampaignsByStoreIdQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetActiveProductCampaignsByStoreIdQueryHandler(
        ICheckpointRepository<CampaignEntity> checkpointRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> submissionWithStoreContainerRepository,
        ICheckpointRepository<SubmissionEntity> submissionChekpointRepository,
        IContainerRepository<ProductViewRecord> productContainerRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetActiveProductCampaignsByStoreIdQuery> validator,
        ILogger<GetActiveProductCampaignsByStoreIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _submissionWithStoreContainerRepository = submissionWithStoreContainerRepository;
        _submissionChekpointRepository = submissionChekpointRepository;
        _productContainerRepository = productContainerRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
        _userService = userService;
    }

    public async Task<Result<List<CampaignProductPerStoreViewRecord>>> Handle(GetActiveProductCampaignsByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all campaigns failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CampaignProductPerStoreViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<CampaignProductPerStoreViewRecord>>);

        if ((request.StoreIds == null || request.StoreIds.Count <= 0) && (request.CampaignIds == null || request.CampaignIds.Count <= 0))
            return result;

        try
        {
            var storeIdsString = request.StoreIds != null ? string.Join(',', request.StoreIds.Select(s => $"'{s}'")) : string.Empty;
            var productIdsString = request.CampaignIds != null ? string.Join(',', request.CampaignIds.Select(s => $"'{s}'")) : string.Empty;
            var query = $"SELECT * FROM c WHERE c.campaignStatus = 'live' AND c.campaignFocusProductIds != null";
            if (!string.IsNullOrEmpty(storeIdsString))
                query += $" AND (ARRAY_CONTAINS(c.storeIds, {storeIdsString}) OR c.storeIds = null)";
            if (!string.IsNullOrEmpty(productIdsString))
                query += $" AND c.id in ({productIdsString})";

            var entities = await _checkpointRepository.QueryAsync(query);
            var campaigns = _mapper.Map<List<CampaignRecord>>(entities.Where(s => s.CampaignEndDate > DateTime.UtcNow));

            var activeCampaigns = new List<CampaignProductPerStoreViewRecord>();
            var submissionsFromAllCampaigns = await GetSubmissionsByCampaignIds(campaigns.Select(s => s.Id).ToList());

            foreach (var campaign in campaigns)
            {
                if (campaign.StoreIds != null)
                    foreach (var storeCampaign in campaign.StoreIds)
                    {
                        if (campaign != null && campaign.CampaignFocusProductIds != null && campaign.CampaignFocusProductIds.Count() > 0)
                        {
                            var submissions = submissionsFromAllCampaigns.Where(s => s.CampaignId == campaign.Id);
                            var categories = await GetProductsByIds(campaign.CampaignFocusProductIds);

                            foreach (var focusProduct in campaign.CampaignFocusProductIds)
                            {
                                var product = categories.FirstOrDefault(s => s.Id == focusProduct);
                                var productSubmissions = new List<CampaignProductSubmissionViewRecord>();

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

                                        if (campaign.RequiredSubmissions <= products.Where(p => p.ItemId == focusProduct.ToString() || p.ItemName == product?.Name).Count())
                                            continue;

                                        productSubmissions.Add(new CampaignProductSubmissionViewRecord()
                                        {
                                            CampaignId = campaign.Id,
                                            StoreId = submission.StoreId,
                                            StoreName = submission.StoreName ?? string.Empty,
                                            UserId = submission.UserId,
                                            SubmissionId = submission.Id,
                                            FocusProductId = focusProduct,
                                            FocusProductName = product?.Name ?? string.Empty
                                        });
                                    }
                                }

                                var campaignToBeAdded = new CampaignProductPerStoreViewRecord()
                                {
                                    Id = campaign.Id,
                                    Name = campaign.Name,
                                    CampaignStatus = campaign.CampaignStatus,
                                    StoreId = storeCampaign,
                                    SubmissionCount = productSubmissions.Count(),
                                    SubmissionTemplateId = campaign?.SelectedTemplateIds?.FirstOrDefault() ?? null,
                                    RequiredSubmissions = campaign?.RequiredSubmissions ?? 0,
                                    CampaignEndDate = campaign?.CampaignEndDate,
                                    CampaignPublishedDate = campaign?.CampaignPublishedDate,
                                    CampaignType = campaign?.CampaignType,
                                    FocusProductId = focusProduct,
                                    FocusProductName = product?.Name ?? string.Empty,
                                    CampaignProductSubmissions = productSubmissions
                                };

                                activeCampaigns.Add(campaignToBeAdded);
                            }
                            //}
                        }
                    }
            }
            var response = request?.StoreIds != null && request?.StoreIds.Count() > 0 ? activeCampaigns?.Where(c => c.RequiredSubmissions > c.SubmissionCount && request.StoreIds.Any(s => s == c.StoreId.Value)) : activeCampaigns?.Where(c => c.RequiredSubmissions > c.SubmissionCount);

            return Result<List<CampaignProductPerStoreViewRecord>>.Success(response?.ToList());

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CampaignProductPerStoreViewRecord>>.Error(ex);
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
    private async Task<List<ProductViewRecord>> GetProductsByIds(List<Guid>? productIds)
    {
        const string productIdKey = "@productId";

        var conditions = new List<string>();
        //var parameters = new Dictionary<string, string>();

        if (productIds != null && productIds.Count > 0)
        {
            var productIdString = string.Join(',', productIds.Select(s => $"'{s}'"));
            conditions.Add($"c.id IN ({productIdString})");
        }

        if (!conditions.Any())
            return new List<ProductViewRecord>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affected = await _productContainerRepository.QueryAsync(sqlQueryText);
        return affected;
    }
}