namespace Pondrop.Service.Submission.Domain.Models;

public record SubmissionViewRecord(
         Guid Id,
        string Title,
        string Description,
        string Icon,
        List<SubmissionStepTemplateRecord> StepTemplates,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : SubmissionTemplateRecord(Id, Title, Description, Icon, StepTemplates, CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public SubmissionViewRecord() : this(
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