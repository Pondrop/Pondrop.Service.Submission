using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;

public class CreateSubmissionTemplateCommandHandlerValidator : AbstractValidator<CreateSubmissionTemplateCommand>
{
    public CreateSubmissionTemplateCommandHandlerValidator()
    {

        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.IconCodePoint).NotEmpty();
        RuleFor(x => x.IconFontFamily).NotEmpty();

        RuleFor(x => x.Steps).NotNull();
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(x => x.Title).NotEmpty();
            step.RuleFor(x => x.Instructions).NotEmpty();
            step.RuleFor(x => x.InstructionsContinueButton).NotEmpty();
            step.RuleFor(x => x.InstructionsIconCodePoint).NotEmpty();
            step.RuleFor(x => x.InstructionsIconFontFamily).NotEmpty();
        });
    }
}