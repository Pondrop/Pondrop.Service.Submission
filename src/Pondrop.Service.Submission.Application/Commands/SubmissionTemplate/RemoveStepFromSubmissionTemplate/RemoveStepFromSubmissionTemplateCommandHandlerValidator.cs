using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.RemoveStepFromSubmission;

public class RemoveStepFromSubmissionTemplateCommandHandlerValidator : AbstractValidator<RemoveStepFromSubmissionTemplateCommand>
{
    public RemoveStepFromSubmissionTemplateCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SubmissionTemplateId).NotEmpty();
    }
}