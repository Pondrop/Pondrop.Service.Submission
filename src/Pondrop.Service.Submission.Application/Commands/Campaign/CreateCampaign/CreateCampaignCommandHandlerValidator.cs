using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateCampaign;

public class CreateCampaignCommandHandlerValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandHandlerValidator()
    {

        RuleFor(x => x.Name).NotEmpty();
    }
}