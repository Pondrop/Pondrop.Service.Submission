using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class CreateSubmissionTemplateCommand : IRequest<Result<SubmissionTemplateRecord>>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public List<StepTemplateRecord?> StepTemplates { get; init; } = default;
}

public record StepTemplateRecord(
    string Title,
    string Type)
{
    public StepTemplateRecord() : this(
        string.Empty,
        string.Empty)
    {
    }
}
