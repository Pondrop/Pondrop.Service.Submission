using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignCategoryPerStoreViewRecord(
        Guid? FocusCategoryId,
        string? FocusCategoryName) : CampaignPerStoreViewRecord
{
    public CampaignCategoryPerStoreViewRecord() : this(
        Guid.Empty,
        string.Empty)
    {
    }
}