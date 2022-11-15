using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignProductPerStoreViewRecord(
        List<CampaignProductSubmissionViewRecord>? CampaignProductSubmissions,
        Guid? FocusProductId,
        string? FocusProductName) : CampaignPerStoreViewRecord
{
    public CampaignProductPerStoreViewRecord() : this(
        null,
        Guid.Empty,
        string.Empty)
    {
    }
}