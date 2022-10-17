using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class AddStepToSubmissionTemplateCommandHandlerValidator : AbstractValidator<AddStepToSubmissionTemplateCommand>
{
    public AddStepToSubmissionTemplateCommandHandlerValidator()
    {
        RuleFor(x => x.Title).NotEmpty();

        RuleFor(x => x.FieldIds).NotNull();
    }
}