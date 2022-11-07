using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetAllCampaignsQuery : IRequest<Result<List<CampaignRecord>>>
{
    public int Limit { get; set; }

    public int Offset { get; set; }
}