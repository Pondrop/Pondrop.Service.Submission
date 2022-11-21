using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreTypeRecord(
        Guid Id,
        string ExternalReferenceId,
        string Name,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public StoreTypeRecord() : this(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, null)
    {
    }
}