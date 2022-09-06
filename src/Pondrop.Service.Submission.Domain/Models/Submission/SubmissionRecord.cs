namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionRecord(
        Guid Id,
        Guid StoreVisitId,
        Guid SubmissionTemplateId,
        double Latitude,
        double Longitude,
        List<SubmissionStepRecord> Steps,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public SubmissionRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        new List<SubmissionStepRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}