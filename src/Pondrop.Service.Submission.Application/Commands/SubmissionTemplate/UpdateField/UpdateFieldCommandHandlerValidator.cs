using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class UpdateFieldCommandHandlerValidator : AbstractValidator<UpdateFieldCommand>
{
    public UpdateFieldCommandHandlerValidator()
    {
    }
}