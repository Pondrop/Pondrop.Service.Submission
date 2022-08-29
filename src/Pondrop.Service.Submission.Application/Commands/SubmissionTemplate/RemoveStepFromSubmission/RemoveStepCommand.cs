using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.RemoveStepFromSubmission;

public class RemoveStepCommand : IRequest<Result<SubmissionTemplateRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public Guid SubmissionTemplateId { get; init; } = Guid.Empty;

    public string UpdatedBy { get; set; } = string.Empty;

}