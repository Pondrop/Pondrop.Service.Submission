namespace Pondrop.Service.Submission.Domain.Models;

public record AuditRecord(string CreatedBy, string UpdatedBy, DateTime CreatedUtc, DateTime UpdatedUtc);