using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissionsWithStore;

public class GetAllSubmissionsWithStoreQuery : IRequest<Result<List<SubmissionWithStoreViewRecord>>>
{
    public int Limit { get; set; }

    public int Offset { get; set; }
}