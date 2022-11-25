using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record StepRecord(
        Guid Id,
        string Title,
        string Instructions,
        List<string> InstructionsStep,
        string InstructionsContinueButton,
        string InstructionsSkipButton,
        int InstructionsIconCodePoint,
        string InstructionsIconFontFamily,
        bool IsSummary,
        List<SubmissionTemplateFieldRecord> FieldDefinitions,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public StepRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        new List<string>(),
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        false,
        new List<SubmissionTemplateFieldRecord>(),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
         null)
    {
    }
}