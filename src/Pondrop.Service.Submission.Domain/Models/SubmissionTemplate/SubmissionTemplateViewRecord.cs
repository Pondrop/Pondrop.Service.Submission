using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record SubmissionTemplateViewRecord(
         Guid Id,
        string Title,
        string Description,
        int IconCodePoint,
        string IconFontFamily,
        SubmissionTemplateType Type,
        SubmissionTemplateStatus Status,
        string? IsForManualSubmissions,
        SubmissionTemplateFocus Focus,
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
        SubmissionTemplateType.unknown,
        SubmissionTemplateStatus.unknown,
        null,
        SubmissionTemplateFocus.unknown,
        new List<StepViewRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}