using FluentValidation;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateStoreVisit;

public class CreateStoreVisitCommandHandlerValidator : AbstractValidator<CreateStoreVisitCommand>
{
    public CreateStoreVisitCommandHandlerValidator()
    {

        RuleFor(x => x.StoreId).NotEmpty();
    }
}