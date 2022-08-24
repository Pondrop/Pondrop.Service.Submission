using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateCheckpointByIdCommandValidator : AbstractValidator<UpdateCheckpointByIdCommand>
{
    public UpdateCheckpointByIdCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}