using Pondrop.Service.Submission.Domain.Enums.StoreVisit;

namespace Pondrop.Service.Submission.Domain.Models.StoreVisit;
public record StoreVisitViewRecord(
        Guid Id,
        Guid StoreId,
        Guid UserId,
        double Latitude,
        double Longitude,
        ShopModeStatus ShopModeStatus,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public StoreVisitViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        ShopModeStatus.Started,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}
