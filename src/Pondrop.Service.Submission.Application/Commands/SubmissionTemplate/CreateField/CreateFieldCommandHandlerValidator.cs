using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class CreateFieldCommandHandlerValidator : AbstractValidator<CreateFieldCommand>
{
    public CreateFieldCommandHandlerValidator()
    {
    }
}