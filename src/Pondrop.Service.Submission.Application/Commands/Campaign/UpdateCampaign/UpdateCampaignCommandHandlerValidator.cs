using FluentValidation;

namespace Pondrop.Service.Campaign.Application.Commands;

public class UpdateCampaignCommandHandlerValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}