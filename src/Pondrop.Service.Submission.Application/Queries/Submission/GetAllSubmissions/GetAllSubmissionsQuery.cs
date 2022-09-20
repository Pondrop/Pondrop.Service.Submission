using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissions;

public class GetAllSubmissionsQuery : IRequest<Result<List<SubmissionViewRecord>>>
{
}