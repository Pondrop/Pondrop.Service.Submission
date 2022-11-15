using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveCategoryCampaignsByStoreIdQueryHandlerValidator : AbstractValidator<GetActiveCategoryCampaignsByStoreIdQuery>
{
    public GetActiveCategoryCampaignsByStoreIdQueryHandlerValidator()
    {
    }
}