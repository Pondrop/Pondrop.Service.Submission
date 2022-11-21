using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Domain.Models.StoreVisit;
public record StoreVisitRecord(
        Guid Id,
        Guid StoreId,
        Guid UserId,
        double Latitude,
        double Longitude,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public StoreVisitRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}