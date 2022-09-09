using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;

public class GetSubmissionByIdQuery : IRequest<Result<SubmissionRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}