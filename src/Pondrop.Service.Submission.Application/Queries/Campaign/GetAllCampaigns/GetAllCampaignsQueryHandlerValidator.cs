using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetAllCampaignsQueryHandlerValidator : AbstractValidator<GetAllCampaignsQuery>
{
    public GetAllCampaignsQueryHandlerValidator()
    {
    }
}