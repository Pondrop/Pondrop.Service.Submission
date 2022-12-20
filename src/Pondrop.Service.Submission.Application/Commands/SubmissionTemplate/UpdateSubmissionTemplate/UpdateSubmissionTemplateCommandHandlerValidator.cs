using FluentValidation;

namespace Pondrop.Service.SubmissionTemplate.Application.Commands;

public class UpdateSubmissionTemplateCommandHandlerValidator : AbstractValidator<UpdateSubmissionTemplateCommand>
{
    public UpdateSubmissionTemplateCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.IconCodePoint).NotEmpty();
        RuleFor(x => x.IconFontFamily).NotEmpty();

        //RuleFor(x => x.Steps).NotNull();
        //RuleForEach(x => x.Steps).ChildRules(step =>
        //{
        //    step.RuleFor(x => x.Title).NotEmpty();
        //});
    }
}