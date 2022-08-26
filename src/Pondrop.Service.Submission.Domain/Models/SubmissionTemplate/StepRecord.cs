namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record StepRecord(
        Guid Id,
        string Title,
        string Instructions,
        string InstructionsContinueButton,
        string InstructionsSkipButton,
        int IconCodePoint,
        string IconFontFamily,
        List<FieldRecord> Fields,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public StepRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        new List<FieldRecord>(),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}