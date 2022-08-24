using FluentValidation;
using Pondrop.Service.Submission.Application.Interfaces.Services;

namespace Pondrop.Service.Submission.Application.Commands;

public class CreateSubmissionTemplateCommandHandlerValidator : AbstractValidator<CreateSubmissionTemplateCommand>
{
    public CreateSubmissionTemplateCommandHandlerValidator()
    {

        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Icon).NotEmpty();

        RuleFor(x => x.StepTemplates).NotNull();
        RuleForEach(x => x.StepTemplates).ChildRules(step =>
        {
            step.RuleFor(x => x.Title).NotEmpty();
            step.RuleFor(x => x.Type).NotEmpty();
        });
    }
}