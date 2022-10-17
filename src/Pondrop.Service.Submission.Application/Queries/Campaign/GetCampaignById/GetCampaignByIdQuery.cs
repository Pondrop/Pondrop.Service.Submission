using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetCampaignById;

public class GetCampaignByIdQuery : IRequest<Result<CampaignRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}