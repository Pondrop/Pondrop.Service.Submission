﻿using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveCategoryCampaignsByStoreIdQuery : IRequest<Result<List<CampaignCategoryPerStoreViewRecord>>>
{
    public List<Guid>? CampaignIds { get; set; } = null;
    public List<Guid>? StoreIds { get; set; } = null;

}