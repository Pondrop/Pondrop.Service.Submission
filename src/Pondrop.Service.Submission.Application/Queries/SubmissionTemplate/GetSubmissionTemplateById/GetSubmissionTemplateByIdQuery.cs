using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetSubmissionTemplateById;

public class GetSubmissionTemplateByIdQuery : IRequest<Result<SubmissionTemplateRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}