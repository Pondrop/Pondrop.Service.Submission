using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitById;

public class GetStoreVisitByIdQuery : IRequest<Result<StoreVisitViewRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}