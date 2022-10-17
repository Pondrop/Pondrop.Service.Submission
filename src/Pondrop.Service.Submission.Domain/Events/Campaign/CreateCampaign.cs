using Pondrop.Service.Submission.Domain.Enums.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        DateTime? CampaignEndDate,
        CampaignStatus? CampaignStatus,
        string PublicationlifecycleId) : EventPayload;
