namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record SubmissionTemplateViewRecord(
         Guid Id,
        string Title,
        string Description,
         int IconCodePoint,
         string IconFontFamily,
         List<StepRecord> Steps,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : SubmissionTemplateRecord(Id, Title, Description, IconCodePoint, IconFontFamily, Steps, CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public SubmissionTemplateViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        new List<StepRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}