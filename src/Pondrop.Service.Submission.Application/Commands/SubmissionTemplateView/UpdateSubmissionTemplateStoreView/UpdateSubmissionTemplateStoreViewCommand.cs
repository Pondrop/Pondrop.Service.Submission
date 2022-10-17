using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionTemplateStoreViewCommand : IRequest<Result<int>>
{
    public Guid? SubmissionTemplateId { get; init; } = null;
}