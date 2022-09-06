using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;

public class CreateSubmissionCommandHandlerValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionCommandHandlerValidator()
    {

        RuleFor(x => x.StoreVisitId).NotEmpty();
        RuleFor(x => x.SubmissionTemplateId).NotEmpty();
        RuleFor(x => x.Latitude).NotEmpty();
        RuleFor(x => x.Longitude).NotEmpty();

        RuleFor(x => x.Steps).NotNull();
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(x => x.TemplateStepId).NotEmpty();
            step.RuleFor(x => x.Latitude).NotEmpty();
            step.RuleFor(x => x.Longitude).NotEmpty();
        });
    }
}