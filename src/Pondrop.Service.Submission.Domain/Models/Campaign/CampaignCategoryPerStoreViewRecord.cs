using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignCategoryPerStoreViewRecord(
        List<CampaignCategorySubmissionViewRecord>? CampaignCategorySubmissions,
        Guid? FocusCategoryId,
        string? FocusCategoryName) : CampaignPerStoreViewRecord
{
    public CampaignCategoryPerStoreViewRecord() : this(
        null,
        Guid.Empty,
        string.Empty)
    {
    }
}