using Pondrop.Service.Events;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;

namespace Pondrop.Service.Submission.Domain.Events.StoreVisit;

public record CreateStoreVisit(
    Guid Id,
    Guid StoreId,
    Guid UserId,
    double Latitude,
    double Longitude,
    ShopModeStatus ShopModeStatus) : EventPayload;
