using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveCampaignsByStoreIdQueryHandlerValidator : AbstractValidator<GetActiveCampaignsByStoreIdQuery>
{
    public GetActiveCampaignsByStoreIdQueryHandlerValidator()
    {
    }
}