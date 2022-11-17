using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignPerStoreViewRecord(
        Guid Id,
        string Name,
        CampaignType? CampaignType,
        CampaignStatus? CampaignStatus,
        Guid? SubmissionTemplateId,
        int RequiredSubmissions,
        Guid? StoreId,
        DateTime? CampaignPublishedDate,
        DateTime? CampaignEndDate,
int SubmissionCount)
{
    public CampaignPerStoreViewRecord() : this(
        Guid.Empty,
        string.Empty,
        null,
        null,
        null,
        0,
        null,
        DateTime.MinValue,
        DateTime.MinValue,
        0)
    {
    }
}