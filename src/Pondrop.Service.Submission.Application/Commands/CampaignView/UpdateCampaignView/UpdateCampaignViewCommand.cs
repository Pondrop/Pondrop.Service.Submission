using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateCampaignViewCommand : IRequest<Result<int>>
{
    public Guid? CampaignId { get; init; } = null;
    public Guid? SubmissionId { get; init; } = null;
}