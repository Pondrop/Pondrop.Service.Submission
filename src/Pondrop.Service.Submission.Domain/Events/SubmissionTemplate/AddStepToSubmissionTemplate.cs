using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record AddStepToSubmissionTemplate(
    Guid Id,
    Guid SubmissionTemplateId,
    string Title,
    string Instructions,
    string InstructionsContinueButton,
    string InstructionsSkipButton,
    int InstructionsIconCodePoint,
    string InstructionsIconFontFamily,
    bool IsSummary,
    List<Guid> FieldIds,
    string CreatedBy,
    string UpdatedBy) : EventPayload;
