using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record AddStep(
    Guid Id,
    Guid SubmissionTemplateId,
    string Title,
    string Instructions,
    string InstructionsContinueButton,
    string InstructionsSkipButton,
    int InstructionsIconCodePoint,
    string InstructionsIconFontFamily,
    List<FieldRecord> Fields,
    string CreatedBy,
    string UpdatedBys) : EventPayload;
