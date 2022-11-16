using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetActiveProductCampaignsByStoreIdQueryHandlerValidator : AbstractValidator<GetActiveProductCampaignsByStoreIdQuery>
{
    public GetActiveProductCampaignsByStoreIdQueryHandlerValidator()
    {
    }
}