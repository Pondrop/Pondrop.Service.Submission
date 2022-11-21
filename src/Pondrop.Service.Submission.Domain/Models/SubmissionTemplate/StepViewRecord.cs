using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record StepViewRecord(
        Guid Id,
        string Title,
        string Instructions,
        string InstructionsContinueButton,
        string InstructionsSkipButton,
        int InstructionsIconCodePoint,
        string InstructionsIconFontFamily,
        bool IsSummary,
        List<FieldRecord> Fields,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public StepViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        false,
        new List<FieldRecord>(),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
         null)
    {
    }
}