using FluentValidation;

namespace Pondrop.Service.StoreVisit.Application.Commands;

public class UpdateStoreVisitCommandHandlerValidator : AbstractValidator<UpdateStoreVisitCommand>
{
    public UpdateStoreVisitCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ShopModeStatus).NotEmpty();
    }
}