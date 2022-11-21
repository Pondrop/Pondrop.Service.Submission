using Pondrop.Service.Events;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;

namespace Pondrop.Service.Submission.Domain.Events.StoreVisit;

public record UpdateStoreVisit(
    Guid Id,
    double Latitude,
    double Longitude,
    ShopModeStatus ShopModeStatus) : EventPayload;
