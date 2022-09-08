using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateStoreVisit;

public class CreateStoreVisitCommand : IRequest<Result<StoreVisitRecord>>
{
    public Guid StoreId { get; init; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

}