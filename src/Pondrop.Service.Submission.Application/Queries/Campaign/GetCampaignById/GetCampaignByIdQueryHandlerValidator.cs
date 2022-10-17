using FluentValidation;
using Pondrop.Service.Submission.Application.Queries.Campaign.GetCampaignById;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetCampaignByIdQueryHandlerValidator : AbstractValidator<GetCampaignByIdQuery>
{
    public GetCampaignByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}