using Microsoft.Azure.Cosmos.Spatial;
using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreAddressRecord(
        Guid Id,
        string ExternalReferenceId,
        string AddressLine1,
        string AddressLine2,
        string Suburb,
        string State,
        string Postcode,
        string Country,
        double Latitude,
        double Longitude,
        Point LocationSort,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public StoreAddressRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        new Point(0, 0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}
