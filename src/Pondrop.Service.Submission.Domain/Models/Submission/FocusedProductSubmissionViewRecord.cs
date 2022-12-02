using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record FocusedProductSubmissionViewRecord(
        Guid Id,
        Guid StoreVisitId,
        Guid SubmissionTemplateId,
        Guid? SubmissionCampaignId,
        Guid StoreId,
        string? StoreName,
        string? StoreRetailerName,
        double StoreLatitude,
        double StoreLongitude,
        string? CampaignName,
        DateTime? CampaignPublishedDate,
        DateTime? CampaignStartDate,
        DateTime? CampaignEndDate,
        string? SubmissionTemplateName,
        double SubmissionLatitude,
        double SubmissionLongitude,
        double SubmissionToStoreDistance,
        string SubmissionStatus,
        Guid FocusProductId,
        string FocusProductName,
        string FocusProductEAN,
        string FocusProductSize,
        string FocusProductBrand,
        List<IdNamePair> FocusProductCategories,
        IdNamePair FocusParentCategory,
        string FocusAisle,
        string FocusSection,
        string FocusShelf,
        string SubmissionAisle,
        string SubmissionSection,
        string SubmissionShelf,
        double SubmissionPrice,
        double SubmissionUnitPrice,
        string SubmissionUnitPriceUOM,
        double SubmissionQuantity,
        DateTime SubmissionNearestUBD,
        string SubmissionIssue,
        string SubmissionComments,
        List<string> SubmissionImages,
        DateTime SubmittedUtc)
{
    public FocusedProductSubmissionViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        DateTime.MinValue,
        string.Empty,
        0,
        0,
        0,
        string.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        new List<IdNamePair>(),
        new IdNamePair(),
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        string.Empty,
        0,
        DateTime.MinValue,
        string.Empty,
        string.Empty,
        new List<string>(),
        DateTime.MinValue)
    {
    }
}

public record IdNamePair(
    Guid Id,
    string Name)
{
    public IdNamePair() : this(Guid.Empty, string.Empty) { }
}
