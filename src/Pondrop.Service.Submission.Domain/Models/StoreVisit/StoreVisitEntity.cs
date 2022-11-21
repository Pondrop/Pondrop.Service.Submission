using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Events.StoreVisit;

namespace Pondrop.Service.Submission.Domain.Models.StoreVisit;

public record StoreVisitEntity : EventEntity
{
    public StoreVisitEntity()
    {
        Id = Guid.Empty;
        StoreId = Guid.Empty;
        UserId = Guid.Empty;
        Latitude = 0;
        Longitude = 0;
        ShopModeStatus = ShopModeStatus.Started;
    }

    public StoreVisitEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public StoreVisitEntity(Guid storeId, Guid userId, double latitude, double longitude, ShopModeStatus shopModeStatus, string createdBy) : this()
    {
        var create = new CreateStoreVisit(Guid.NewGuid(), storeId, userId, latitude, longitude, shopModeStatus);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "storeId")]
    public Guid StoreId { get; private set; }

    [JsonProperty(PropertyName = "userId")]
    public Guid UserId { get; private set; }

    [JsonProperty("latitude")]
    public double Latitude { get; private set; }

    [JsonProperty("longitude")]
    public double Longitude { get; private set; }

    [JsonProperty("shopModeStatus")]
    public ShopModeStatus ShopModeStatus { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateStoreVisit create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateStoreVisit update:
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
        if (eventPayloadToApply is CreateStoreVisit create)
        {
            Apply(new Event(GetStreamId<StoreVisitEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateStoreVisit create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        StoreId = create.StoreId;
        UserId = create.UserId;
        Latitude = create.Latitude;
        Longitude = create.Longitude;
        ShopModeStatus = create.ShopModeStatus;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateStoreVisit update, string createdBy, DateTime createdUtc)
    {
        {
            var oldLatitude = Latitude;
            var oldLongitude = Longitude;
            var oldShopModeStatus = ShopModeStatus;

            Latitude = update.Latitude;
            Longitude = update.Longitude;
            ShopModeStatus = update.ShopModeStatus;

            if (oldLatitude != Latitude ||
                oldLongitude != Longitude ||
                oldShopModeStatus != ShopModeStatus)
            {
                UpdatedBy = createdBy;
                UpdatedUtc = createdUtc;
            }
        }
    }

}