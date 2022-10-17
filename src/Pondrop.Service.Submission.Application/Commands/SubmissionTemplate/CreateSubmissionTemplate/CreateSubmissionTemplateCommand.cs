using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;

public class CreateSubmissionTemplateCommand : IRequest<Result<SubmissionTemplateRecord>>
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int IconCodePoint { get; set; } = int.MaxValue;

    public string IconFontFamily { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public List<StepRecord?> Steps { get; init; } = default;
}

public record StepRecord(
    Guid Id,
    string Title,
    string Instructions,
    string InstructionsContinueButton,
    string InstructionsSkipButton,
    int InstructionsIconCodePoint,
    string InstructionsIconFontFamily,
    bool IsSummary,
    List<Guid> FieldIds,
    string CreatedBy)
{
    public StepRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        false,
        new List<Guid>(),
        string.Empty)
    {
    }
}


