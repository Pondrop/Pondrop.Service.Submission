using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetAllSubmissionTemplatesQuery : IRequest<Result<List<SubmissionViewRecord>>>
{
}