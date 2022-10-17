using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class AddStepToSubmissionTemplateCommand : IRequest<Result<SubmissionTemplateRecord>>
{
    public Guid SubmissionId { get; init; } = Guid.Empty;

    public string Title { get; init; } = string.Empty;

    public string Instructions { get; init; } = string.Empty;

    public string InstructionsContinueButton { get; init; } = string.Empty;

    public string InstructionsSkipButton { get; init; } = string.Empty;

    public int InstructionsIconCodePoint { get; init; } = int.MinValue;

    public string InstructionsIconFontFamily { get; init; } = string.Empty;
    public bool IsSummary { get; init; }

    public string CreatedBy { get; set; } = string.Empty;

    public List<Guid> FieldIds { get; set; } = default;

}