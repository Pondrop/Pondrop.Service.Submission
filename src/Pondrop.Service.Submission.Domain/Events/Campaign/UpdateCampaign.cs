using Pondrop.Service.Events;
using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Events.Campaign;

public record UpdateCampaign(
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
        string PublicationlifecycleId) : EventPayload;
