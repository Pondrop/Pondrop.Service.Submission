using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Domain.Models.Submission;

public record SubmissionStepRecord(
        Guid Id,
        Guid TemplateStepId,
        double Latitude,
        double Longitude,
        List<SubmissionFieldRecord> Fields,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public SubmissionStepRecord() : this(
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        new List<SubmissionFieldRecord>(),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}