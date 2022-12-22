using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Product;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildFocusedProductSubmissionViewCommandHandler : IRequestHandler<RebuildFocusedProductSubmissionViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<StoreVisitEntity> _storeVisitCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<StoreViewRecord> _storeContainerRepository;
    private readonly IContainerRepository<ProductViewRecord> _productContainerRepository;
    private readonly IContainerRepository<FocusedProductSubmissionViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly PriceTemplateConfig _priceTemplateConfig;
    private readonly ICheckpointRepository<CampaignEntity> _campaignCheckpointRepository;
    private readonly QuickPriceAndQuantityTemplateConfig _quickPriceAndQuantityTemplateConfig;
    private readonly ShelfStockLevelsTemplateConfig _shelfStockLevelsTemplateConfig;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildFocusedProductSubmissionViewCommandHandler> _logger;

    public RebuildFocusedProductSubmissionViewCommandHandler(
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        IContainerRepository<StoreViewRecord> storeContainerRepository,
        ICheckpointRepository<CampaignEntity> campaignCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<FocusedProductSubmissionViewRecord> containerRepository,
        IContainerRepository<ProductViewRecord> productContainerRepository,
        IOptions<PriceTemplateConfig> priceTemplateConfig,
        IOptions<QuickPriceAndQuantityTemplateConfig> quickPriceAndQuantityTemplateConfig,
        IOptions<ShelfStockLevelsTemplateConfig> shelfStockLevelsTemplateConfig,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildFocusedProductSubmissionViewCommandHandler> logger) : base()
    {
        _quickPriceAndQuantityTemplateConfig = quickPriceAndQuantityTemplateConfig.Value;
        _shelfStockLevelsTemplateConfig = shelfStockLevelsTemplateConfig.Value;
        _priceTemplateConfig = priceTemplateConfig.Value;
        _campaignCheckpointRepository = campaignCheckpointRepository;
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _productContainerRepository = productContainerRepository;
        _storeVisitCheckpointRepository = storeVisitCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _storeContainerRepository = storeContainerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildFocusedProductSubmissionViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var storeVisitTask = _storeVisitCheckpointRepository.GetAllAsync();
            var submissionTemplateTask = _submissionTemplateCheckpointRepository.GetAllAsync();
            var submissionsTask = _submissionCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.submissionTemplateId in ('{_priceTemplateConfig.Id}','{_shelfStockLevelsTemplateConfig.Id}','{_quickPriceAndQuantityTemplateConfig.Id}')");
            //var submissionsTask = _submissionCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.campaignId != null");

            await Task.WhenAll(storeVisitTask, submissionTemplateTask, submissionsTask);

            var storeVisitLookup = storeVisitTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<StoreVisitViewRecord>(i));
            var submissionTemplateLookup = submissionTemplateTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<SubmissionTemplateRecord>(i));


            var tasks = submissionsTask.Result.Select(async i =>
        {
            var success = false;

            try
            {
                var storeVisit = storeVisitLookup[i.StoreVisitId];
                var store = await _storeContainerRepository.GetByIdAsync(storeVisit.StoreId);
                CampaignEntity? campaign = null;

                if (i.CampaignId.HasValue)
                    campaign = await _campaignCheckpointRepository.GetByIdAsync(i.CampaignId.Value);

                var fields = await ExtractFieldValues(i.SubmissionTemplateId, i.Steps);

                var submissionView = fields with
                {
                    Id = i.Id,
                    SubmissionTemplateId = i.SubmissionTemplateId,
                    StoreVisitId = i.StoreVisitId,
                    StoreId = storeVisit.StoreId,
                    SubmissionCampaignId = i.CampaignId ?? Guid.Empty,
                    CampaignName = campaign?.Name ?? String.Empty,
                    CampaignPublishedDate = campaign?.CampaignPublishedDate ?? DateTime.MinValue,
                    CampaignEndDate = campaign?.CampaignEndDate ?? DateTime.MinValue,
                    StoreName = store?.Name ?? string.Empty,
                    StoreRetailerName = store?.Retailer?.Name ?? string.Empty,
                    StoreLatitude = store?.Addresses?.FirstOrDefault()?.Latitude ?? 0,
                    StoreLongitude = store?.Addresses?.FirstOrDefault()?.Longitude ?? 0,
                    SubmissionLatitude = i.Latitude,
                    SubmissionLongitude = i.Longitude,
                    SubmittedUtc = i.CreatedUtc,
                    SubmissionStatus = string.Empty,
                    SubmissionToStoreDistance = CalculateDistance(i.Latitude, i.Longitude, store?.Addresses?.FirstOrDefault()?.Latitude, store?.Addresses?.FirstOrDefault()?.Longitude)
                };



                var result = await _containerRepository.UpsertAsync(submissionView);
                success = result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update submission view for '{i.Id}'");
            }

            return success;
        }).ToList();

            await Task.WhenAll(tasks);

            result = Result<int>.Success(tasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to rebuild submission view");
            result = Result<int>.Error(ex);
        }

        return result;
    }

    private double CalculateDistance(double? storeLatitude, double? storeLongitude, double? submissionLatitude, double? submissionLongitude)
    {
        double distance = 0;
        if (storeLatitude.HasValue && storeLongitude.HasValue && submissionLatitude.HasValue && submissionLongitude.HasValue)
        {
            double x = 69.1 * (storeLatitude.Value - submissionLatitude.Value);
            double y = 69.1 * (storeLongitude.Value - submissionLongitude.Value) * Math.Cos(storeLatitude.Value / 57.3);

            return Math.Round((Math.Sqrt(x * x + y * y) * 1.609344) / 0.001);

        }
        return distance;
    }

    private async Task<FocusedProductSubmissionViewRecord?> ExtractFieldValues(Guid submissionTemplateId, List<SubmissionStepRecord> steps)
    {
        FocusedProductSubmissionViewRecord? fields = null;

        if (submissionTemplateId == _priceTemplateConfig.Id)
        {
            var product = new ItemValueRecord();
            double productPrice = -999;
            var labelProduct = string.Empty;
            var labelBarcode = string.Empty;
            var priceType = string.Empty;
            var unitPriceUOM = string.Empty;
            var photoUrl = string.Empty;
            double unitPrice = -999;
            var comment = string.Empty;
            var productEAN = string.Empty;
            var productSize = string.Empty;
            var productCategories = new List<IdNamePair>();
            var parentCategory = new IdNamePair();

            foreach (var step in steps)
            {
                foreach (var field in step.Fields)
                {
                    if (field != null)
                    {
                        if (field.TemplateFieldId == _priceTemplateConfig.SearchProductFieldId)
                        {
                            product = field?.Values?.FirstOrDefault()?.ItemValue;

                            var productFull = await _productContainerRepository.GetByIdAsync(new Guid(product.ItemId));
                            if (productFull != null)
                            {
                                productEAN = productFull.BarcodeNumber ?? String.Empty;
                                productSize = $"{productFull.NetContent} {productFull.NetContentUom}";
                                parentCategory = new IdNamePair(productFull?.ParentCategory?.Id ?? Guid.Empty, productFull?.ParentCategory?.Name ?? string.Empty);

                                if (productFull?.Categories != null)
                                    foreach (var productCategory in productFull.Categories)
                                    {
                                        productCategories.Add(new IdNamePair(productCategory?.Id ?? Guid.Empty, productCategory?.Name ?? string.Empty));
                                    }
                            }
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.ProductPriceFieldId)
                        {
                            productPrice = field?.Values?.FirstOrDefault()?.DoubleValue ?? -999;
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.LabelProductNameFieldId)
                        {
                            labelProduct = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.LabelBarcodeFieldId)
                        {
                            labelBarcode = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.PriceTypeFieldId)
                        {
                            priceType = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.UnitPriceFieldId)
                        {
                            unitPrice = field?.Values?.FirstOrDefault()?.DoubleValue ?? -999;
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.UnitPriceUOMFieldId)
                        {
                            unitPriceUOM = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.PhotoFieldId)
                        {
                            photoUrl = field?.Values?.FirstOrDefault()?.PhotoUrl ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _priceTemplateConfig.CommentFieldId)
                        {
                            comment = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                    }
                }
            }

            fields = new FocusedProductSubmissionViewRecord()
            {
                SubmissionUnitPrice = unitPrice,
                SubmissionUnitPriceUOM = unitPriceUOM,
                SubmissionImages = new List<string>() { photoUrl },
                SubmissionPrice = productPrice,
                SubmissionComments = comment,
                FocusProductId = product?.ItemId != null ? new Guid(product.ItemId) : Guid.Empty,
                FocusProductName = product?.ItemName != null ? product.ItemName : string.Empty,
                FocusProductCategories = productCategories,
                FocusParentCategory = parentCategory,
                FocusProductEAN = productEAN,
                FocusProductSize = productSize
            };
        }
        else if (submissionTemplateId == _quickPriceAndQuantityTemplateConfig.Id)
        {
            var product = new ItemValueRecord();
            double productPrice = -999;
            double quantity = -999;
            var aisle = string.Empty;
            var shelfNumber = string.Empty;
            var shelfSection = string.Empty;
            var shelfIssue = string.Empty;
            var priceType = string.Empty;
            var photoUrl = string.Empty;
            var comment = string.Empty;
            var productEAN = string.Empty;
            var productSize = string.Empty;
            var productCategories = new List<IdNamePair>();
            var parentCategory = new IdNamePair();

            foreach (var step in steps)
            {
                foreach (var field in step.Fields)
                {
                    if (field != null)
                    {
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.SearchProductFieldId)
                        {
                            product = field?.Values?.FirstOrDefault()?.ItemValue;

                            var productFull = await _productContainerRepository.GetByIdAsync(new Guid(product.ItemId));
                            if (productFull != null)
                            {
                                productEAN = productFull.BarcodeNumber ?? String.Empty;
                                productSize = $"{productFull.NetContent} {productFull.NetContentUom}";
                                parentCategory = new IdNamePair(productFull?.ParentCategory?.Id ?? Guid.Empty, productFull?.ParentCategory?.Name ?? string.Empty);

                                if (productFull?.Categories != null)
                                    foreach (var productCategory in productFull.Categories)
                                    {
                                        productCategories.Add(new IdNamePair(productCategory?.Id ?? Guid.Empty, productCategory?.Name ?? string.Empty));
                                    }
                            }
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.ProductPriceFieldId)
                        {
                            productPrice = field?.Values?.FirstOrDefault()?.DoubleValue ?? -999;
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.AisleFieldId)
                        {
                            aisle = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.ShelfSectionFieldId)
                        {
                            shelfSection = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.ShelfNumberFieldId)
                        {
                            shelfNumber = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.QuantityFieldId)
                        {
                            quantity = field?.Values?.FirstOrDefault()?.IntValue ?? -999;
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.ShelfIssueFieldId)
                        {
                            shelfIssue = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.PhotoFieldId)
                        {
                            photoUrl = field?.Values?.FirstOrDefault()?.PhotoUrl ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _quickPriceAndQuantityTemplateConfig.CommentFieldId)
                        {
                            comment = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                    }
                }
            }

            fields = new FocusedProductSubmissionViewRecord()
            {
                SubmissionImages = new List<string>() { photoUrl },
                SubmissionPrice = productPrice,
                SubmissionComments = comment,
                SubmissionAisle = aisle,
                SubmissionQuantity = quantity,
                SubmissionShelf = shelfSection,
                SubmissionIssue = shelfIssue,
                FocusProductId = product?.ItemId != null ? new Guid(product.ItemId) : Guid.Empty,
                FocusProductName = product?.ItemName != null ? product.ItemName : string.Empty,
                FocusProductCategories = productCategories,
                FocusParentCategory = parentCategory,
                FocusProductEAN = productEAN,
                FocusProductSize = productSize
            };
        }
        else if (submissionTemplateId == _shelfStockLevelsTemplateConfig.Id)
        {
            var product = new ItemValueRecord();
            DateTime nearestUseByDate = DateTime.MinValue;
            DateTime furthestUseByDate = DateTime.MinValue;
            double quantity = -999;
            double quantityNearestUseByDate = -999;
            double quantityFurthestUseByDate = -999;
            var aisle = string.Empty;
            var shelfNumber = string.Empty;
            var shelfSection = string.Empty;
            var photoUrl = string.Empty;
            var comment = string.Empty;
            var productEAN = string.Empty;
            var productSize = string.Empty;
            var productCategories = new List<IdNamePair>();
            var parentCategory = new IdNamePair();

            foreach (var step in steps)
            {
                foreach (var field in step.Fields)
                {
                    if (field != null)
                    {
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.SearchProductFieldId)
                        {
                            product = field?.Values?.FirstOrDefault()?.ItemValue;
                            if (product?.ItemId != null && product?.ItemId != Guid.Empty.ToString())
                            {
                                var productFull = await _productContainerRepository.GetByIdAsync(new Guid(product.ItemId));
                                if (productFull != null)
                                {
                                    productEAN = productFull.BarcodeNumber ?? String.Empty;
                                    productSize = $"{productFull.NetContent} {productFull.NetContentUom}";
                                    parentCategory = new IdNamePair(productFull?.ParentCategory?.Id ?? Guid.Empty, productFull?.ParentCategory?.Name ?? string.Empty);

                                    if (productFull?.Categories != null)
                                        foreach (var productCategory in productFull.Categories)
                                        {
                                            productCategories.Add(new IdNamePair(productCategory?.Id ?? Guid.Empty, productCategory?.Name ?? string.Empty));
                                        }
                                }
                            }
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.NearestUseByDateFieldId)
                        {
                            nearestUseByDate = field?.Values?.FirstOrDefault()?.DateTimeValue ?? DateTime.MinValue;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.FurthestUseByDateFieldId)
                        {
                            furthestUseByDate = field?.Values?.FirstOrDefault()?.DateTimeValue ?? DateTime.MinValue;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.QuantityAtNearestUseByDateFieldId)
                        {
                            quantityNearestUseByDate = field?.Values?.FirstOrDefault()?.IntValue ?? -999;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.QuantityAtFurthestUseByDateFieldId)
                        {
                            quantityFurthestUseByDate = field?.Values?.FirstOrDefault()?.IntValue ?? -999;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.AisleFieldId)
                        {
                            aisle = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.ShelfSectionFieldId)
                        {
                            shelfSection = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.ShelfNumberFieldId)
                        {
                            shelfNumber = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.QuantityFieldId)
                        {
                            quantity = field?.Values?.FirstOrDefault()?.IntValue ?? -999;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.PhotoFieldId)
                        {
                            photoUrl = field?.Values?.FirstOrDefault()?.PhotoUrl ?? string.Empty;
                        }
                        if (field.TemplateFieldId == _shelfStockLevelsTemplateConfig.CommentFieldId)
                        {
                            comment = field?.Values?.FirstOrDefault()?.StringValue ?? string.Empty;
                        }
                    }
                }
            }

            fields = new FocusedProductSubmissionViewRecord()
            {
                SubmissionImages = new List<string>() { photoUrl },
                SubmissionComments = comment,
                SubmissionAisle = aisle,
                SubmissionNearestUBD = nearestUseByDate,
                SubmissionQuantity = quantity,
                SubmissionShelf = shelfSection,
                FocusProductId = product?.ItemId != null ? new Guid(product.ItemId) : Guid.Empty,
                FocusProductName = product?.ItemName != null ? product.ItemName : string.Empty,
                FocusProductCategories = productCategories,
                FocusParentCategory = parentCategory,
                FocusProductEAN = productEAN,
                FocusProductSize = productSize
            };
        }

        return fields;
    }

}