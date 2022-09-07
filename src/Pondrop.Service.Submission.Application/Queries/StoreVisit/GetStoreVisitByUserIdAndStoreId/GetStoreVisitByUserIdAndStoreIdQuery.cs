using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitByUserIdAndStoreId;

public class GetStoreVisitByUserIdAndStoreIdQuery : IRequest<Result<StoreVisitViewRecord?>>
{
    public Guid UserId { get; init; } = Guid.Empty;
    public Guid StoreId { get; init; } = Guid.Empty;
}