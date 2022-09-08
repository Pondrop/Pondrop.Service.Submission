using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitByStoreId;

public class GetStoreVisitByStoreIdQuery : IRequest<Result<List<StoreVisitViewRecord?>>>
{
    public Guid StoreId { get; init; } = Guid.Empty;
}