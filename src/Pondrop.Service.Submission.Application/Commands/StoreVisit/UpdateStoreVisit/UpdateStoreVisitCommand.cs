using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Commands;

public class UpdateStoreVisitCommand : IRequest<Result<StoreVisitRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public double? Latitude { get; init; } = null;
    public double? Longitude { get; init; } = null;
    public ShopModeStatus? ShopModeStatus { get; init; } = null;
}