using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionWithStoreViewCommand : IRequest<Result<int>>
{
    public Guid? StoreId { get; init; } = null;
    public Guid? SubmissionId { get; init; } = null;
    public string? Name { get; init; } = null;
    public string? RetailerName { get; init; } = null;
}