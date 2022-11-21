using Pondrop.Service.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;

namespace Pondrop.Service.Submission.Domain.Models.StoreVisit;
public record StoreVisitViewRecord(
        Guid Id,
        Guid StoreId,
        Guid UserId,
        StoreViewRecord Store,
        double Latitude,
        double Longitude,
        ShopModeStatus ShopModeStatus,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public StoreVisitViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        new StoreViewRecord(),
        0,
        0,
        ShopModeStatus.Started,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}
