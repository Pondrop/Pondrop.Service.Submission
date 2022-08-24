namespace Pondrop.Service.Submission.Domain.Models;

public record SubmissionTemplateRecord(
        Guid Id,
        string Title,
        string Description,
        string Icon,
        List<SubmissionStepTemplateRecord> StepTemplates,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public SubmissionTemplateRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        new List<SubmissionStepTemplateRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}