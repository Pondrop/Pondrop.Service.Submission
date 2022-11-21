using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignRecord(
        Guid Id,
        string Name,
        CampaignType? CampaignType,
        List<Guid>? CampaignTriggerIds,
        List<Guid>? CampaignFocusCategoryIds,
        List<Guid>? CampaignFocusProductIds,
        List<Guid>? SelectedTemplateIds,
        List<Guid>? StoreIds,
        int RequiredSubmissions,
        Guid? RewardSchemeId,
        DateTime? CampaignPublishedDate,
        DateTime? CampaignEndDate,
        CampaignStatus? CampaignStatus,
        string PublicationlifecycleId,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public CampaignRecord() : this(
        Guid.Empty,
        string.Empty,
        null,
        new List<Guid>(),
        new List<Guid>(),
        new List<Guid>(),
        new List<Guid>(),
        new List<Guid>(),
        0,
        Guid.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}
