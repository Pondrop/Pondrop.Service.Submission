using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class AddStepTemplateToSubmissionCommand : IRequest<Result<SubmissionTemplateRecord>>
{
    public Guid SubmissionId { get; init; } = Guid.Empty;
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}