using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveCampaignsByStoreIdQuery : IRequest<Result<List<CampaignRecord>>>
{
    public List<Guid> StoreIds { get; set; }

}