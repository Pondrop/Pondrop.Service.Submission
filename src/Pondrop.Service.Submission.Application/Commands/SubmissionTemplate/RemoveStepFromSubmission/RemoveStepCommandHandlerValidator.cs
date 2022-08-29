using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.RemoveStepFromSubmission;

public class RemoveStepCommandHandlerValidator : AbstractValidator<RemoveStepCommand>
{
    public RemoveStepCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SubmissionTemplateId).NotEmpty();
    }
}