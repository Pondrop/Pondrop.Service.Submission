using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignPerStoreViewRecord(
        Guid Id,
        string Name,
        CampaignType? CampaignType,
        CampaignStatus? CampaignStatus,
        int RequiredSubmissions,
        List<CampaignCategorySubmissionViewRecord>? CampaignCategorySubmissions,
        Guid? StoreId,
        DateTime? CampaignPublishedDate,
        DateTime? CampaignEndDate)
{
    public CampaignPerStoreViewRecord() : this(
        Guid.Empty,
        string.Empty,
        null,
        null,
        0,
        null,
        null,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}