using Pondrop.Service.Events;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record AddStepToSubmissionTemplate(
    Guid Id,
    Guid SubmissionTemplateId,
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
    string UpdatedBy) : EventPayload;
