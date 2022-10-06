using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class AddStepToSubmissionTemplateCommandHandlerValidator : AbstractValidator<AddStepToSubmissionTemplateCommand>
{
    public AddStepToSubmissionTemplateCommandHandlerValidator()
    {
        RuleFor(x => x.Title).NotEmpty();

        RuleFor(x => x.Fields).NotNull();
        RuleForEach(x => x.Fields).ChildRules(field =>
        {
            field.RuleFor(x => x.Label).NotEmpty();
            field.RuleFor(x => x.FieldType).NotEmpty();
        });
    }
}