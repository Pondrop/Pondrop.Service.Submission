using FluentValidation;
using Pondrop.Service.Submission.Application.Queries.Field.GetFieldById;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetFieldByIdQueryHandlerValidator : AbstractValidator<GetFieldByIdQuery>
{
    public GetFieldByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}