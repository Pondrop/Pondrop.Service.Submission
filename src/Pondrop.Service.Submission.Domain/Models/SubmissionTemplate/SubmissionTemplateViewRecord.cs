namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record SubmissionTemplateViewRecord(
         Guid Id,
        string Title,
        string Description,
         int IconCodePoint,
         string IconFontFamily,
         List<StepViewRecord> Steps,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
{
    public SubmissionTemplateViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        new List<StepViewRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}