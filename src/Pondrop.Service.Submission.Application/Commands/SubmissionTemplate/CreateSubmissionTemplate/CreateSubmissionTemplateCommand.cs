using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;

public class CreateSubmissionTemplateCommand : IRequest<Result<SubmissionTemplateRecord>>
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int IconCodePoint { get; set; } = int.MaxValue;

    public string IconFontFamily { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public SubmissionTemplateType Type { get; set; } = SubmissionTemplateType.unknown;
    public SubmissionTemplateStatus Status { get; set; } = SubmissionTemplateStatus.unknown;
    public SubmissionTemplateFocus Focus { get; set; } = SubmissionTemplateFocus.unknown;
    public SubmissionTemplateInitiationType InitiatedBy { get; set; } = SubmissionTemplateInitiationType.shopper;
    public bool? IsForManualSubmissions { get; set; } = null;

    public List<StepRecord?> Steps { get; init; } = default;
}

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
    string CreatedBy)
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
        string.Empty)
    {
    }
}
