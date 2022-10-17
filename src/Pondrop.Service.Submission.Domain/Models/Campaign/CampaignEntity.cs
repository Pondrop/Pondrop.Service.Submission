using Newtonsoft.Json;
using Pondrop.Service.Submission.Domain.Enums.Campaign;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Events.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;

public record CampaignEntity : EventEntity
{
    public CampaignEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        CampaignType = null;
        CampaignTriggerIds = null;
        CampaignFocusCategoryIds = null;
        CampaignFocusProductIds = null;
        SelectedTemplateIds = null;
        StoreIds = null;
        RequiredSubmissions = 0;
        RewardSchemeId = Guid.Empty;
        CampaignEndDate = null;
        CampaignStatus = null;
        PublicationlifecycleId = string.Empty;
    }

    public CampaignEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public CampaignEntity(string name,
        CampaignType? campaignType,
        List<Guid>? campaignTriggerIds,
        List<Guid>? campaignFocusCategoryIds,
        List<Guid>? campaignFocusProductIds,
        List<Guid>? selectedTemplateIds,
        List<Guid>? storeIds,
        int requiredSubmissions,
        Guid? rewardSchemeId,
        DateTime? campaignEndDate,
        CampaignStatus? campaignStatus,
        string publicationlifecycleId, string createdBy) : this()
    {
        var create = new CreateCampaign(Guid.NewGuid(),
                                        name,
                                        campaignType,
                                        campaignTriggerIds,
                                        campaignFocusCategoryIds,
                                        campaignFocusProductIds,
                                        selectedTemplateIds,
                                        storeIds,
                                        requiredSubmissions,
                                        rewardSchemeId,
                                        campaignEndDate,
                                        campaignStatus,
                                        publicationlifecycleId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [JsonProperty(PropertyName = "campaignType")]
    public CampaignType? CampaignType { get; private set; }

    [JsonProperty("campaignTriggerIds")]
    public List<Guid>? CampaignTriggerIds { get; private set; }

    [JsonProperty("campaignFocusCategoryIds")]
    public List<Guid>? CampaignFocusCategoryIds { get; private set; }

    [JsonProperty("campaignFocusProductIds")]
    public List<Guid>? CampaignFocusProductIds { get; private set; }

    [JsonProperty("selectedTemplateIds")]
    public List<Guid>? SelectedTemplateIds { get; private set; }

    [JsonProperty("storeIds")]
    public List<Guid>? StoreIds { get; private set; }

    [JsonProperty("requiredSubmissions")]
    public int RequiredSubmissions { get; private set; }

    [JsonProperty("rewardSchemeId")]
    public Guid? RewardSchemeId { get; private set; }

    [JsonProperty("campaignEndDate")]
    public DateTime? CampaignEndDate { get; private set; }

    [JsonProperty("campaignStatus")]
    public CampaignStatus? CampaignStatus { get; private set; }

    [JsonProperty("publicationlifecycleId")]
    public string PublicationlifecycleId { get; private set; }



    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateCampaign create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateCampaign update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateCampaign create)
        {
            Apply(new Event(GetStreamId<CampaignEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateCampaign create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        CampaignType = create.CampaignType;
        CampaignTriggerIds = create.CampaignTriggerIds;
        CampaignFocusCategoryIds = create.CampaignFocusCategoryIds;
        CampaignFocusProductIds = create.CampaignFocusProductIds;
        SelectedTemplateIds = create.SelectedTemplateIds;
        StoreIds = create.StoreIds;
        RequiredSubmissions = create.RequiredSubmissions;
        RewardSchemeId = create.RewardSchemeId;
        CampaignEndDate = create.CampaignEndDate;
        CampaignStatus = create.CampaignStatus;
        PublicationlifecycleId = create.PublicationlifecycleId;
    }

    private void When(UpdateCampaign update, string createdBy, DateTime createdUtc)
    {
        {
            var oldName = Name;
            var oldCampaignType = CampaignType;
            var oldCampaignTriggerIds = CampaignTriggerIds;
            var oldCampaignFocusCategoryIds = CampaignFocusCategoryIds;
            var oldCampaignFocusProductIds = CampaignFocusProductIds;
            var oldSelectedTemplateIds = SelectedTemplateIds;
            var oldStoreIds = StoreIds;
            var oldRequiredSubmissions = RequiredSubmissions;
            var oldRewardSchemeId = RewardSchemeId;
            var oldCampaignEndDate = CampaignEndDate;
            var oldCampaignStatus = CampaignStatus;
            var oldPublicationlifecycleId = PublicationlifecycleId;

            Name = update.Name;
            CampaignType = update.CampaignType;
            CampaignTriggerIds = update.CampaignTriggerIds;
            CampaignFocusCategoryIds = update.CampaignFocusCategoryIds;
            CampaignFocusProductIds = update.CampaignFocusProductIds;
            SelectedTemplateIds = update.SelectedTemplateIds;
            StoreIds = update.StoreIds;
            RequiredSubmissions = update.RequiredSubmissions;
            RewardSchemeId = update.RewardSchemeId;
            CampaignEndDate = update.CampaignEndDate;
            CampaignStatus = update.CampaignStatus;
            PublicationlifecycleId = update.PublicationlifecycleId;

            if (oldName != Name ||
                oldCampaignType != CampaignType ||
                oldCampaignTriggerIds != CampaignTriggerIds ||
                oldCampaignFocusCategoryIds != CampaignFocusCategoryIds ||
                oldCampaignFocusProductIds != CampaignFocusProductIds ||
                oldSelectedTemplateIds != SelectedTemplateIds ||
                oldStoreIds != StoreIds ||
                oldRequiredSubmissions != RequiredSubmissions ||
                oldRewardSchemeId != RewardSchemeId ||
                oldCampaignEndDate != CampaignEndDate ||
                oldCampaignStatus != CampaignStatus ||
                oldPublicationlifecycleId != PublicationlifecycleId)
            {
                UpdatedBy = createdBy;
                UpdatedUtc = createdUtc;
            }
        }
    }

}