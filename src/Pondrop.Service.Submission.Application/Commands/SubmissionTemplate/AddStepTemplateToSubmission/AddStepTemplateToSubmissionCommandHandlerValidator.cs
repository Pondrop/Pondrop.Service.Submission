using FluentValidation;
using Pondrop.Service.Submission.Application.Interfaces.Services;

namespace Pondrop.Service.Submission.Application.Commands;

public class AddStepTemplateToSubmissionCommandHandlerValidator : AbstractValidator<AddStepTemplateToSubmissionCommand>
{
    public AddStepTemplateToSubmissionCommandHandlerValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
    }
}