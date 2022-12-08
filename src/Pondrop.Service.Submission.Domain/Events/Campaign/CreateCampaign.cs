using Pondrop.Service.Events;
using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Events.Campaign;

public record CreateCampaign(
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
        DateTime? CampaignStartDate,
        int? minimumTimeIntervalMins,
        int? repeatEvery,
        RepeatEveryUOM? repeatEveryUOM,
        CampaignStatus? CampaignStatus,
        string PublicationlifecycleId) : EventPayload;
