using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class AddStepToSubmissionTemplateCommandHandlerValidator : AbstractValidator<AddStepToSubmissionTemplateCommand>
{
    public AddStepToSubmissionTemplateCommandHandlerValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Instructions).NotEmpty();
        RuleFor(x => x.InstructionsContinueButton).NotEmpty();
        RuleFor(x => x.InstructionsSkipButton).NotEmpty();
        RuleFor(x => x.InstructionsIconCodePoint).NotEmpty();
        RuleFor(x => x.InstructionsIconFontFamily).NotEmpty();

        RuleFor(x => x.Fields).NotNull();
        RuleForEach(x => x.Fields).ChildRules(field =>
        {
            field.RuleFor(x => x.Label).NotEmpty();
            field.RuleFor(x => x.FieldType).NotEmpty();
        });
    }
}