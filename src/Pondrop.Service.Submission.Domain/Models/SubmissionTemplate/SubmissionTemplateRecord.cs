using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record SubmissionTemplateRecord(
        Guid Id,
        string Title,
        string Description,
        int IconCodePoint,
        string IconFontFamily,
        SubmissionTemplateType Type,
        List<StepRecord> Steps,
        SubmissionTemplateInitiationType InitiatedBy,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public SubmissionTemplateRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        SubmissionTemplateType.unknown,
        new List<StepRecord>(0),
        SubmissionTemplateInitiationType.unknown,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
         null)
    {
    }
}