using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSuggestedFieldValueViewCommand : IRequest<Result<int>>
{
    public Guid? SubmissionId { get; init; } = null;
    public Guid? SubmissionTemplateId { get; init; } = null;
    public Guid? StoreVisitId { get; init; } = null;
}