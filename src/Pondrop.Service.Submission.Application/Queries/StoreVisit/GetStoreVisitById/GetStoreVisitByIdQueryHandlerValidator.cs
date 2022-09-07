using FluentValidation;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitById;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetStoreVisitByIdQueryHandlerValidator : AbstractValidator<GetStoreVisitByIdQuery>
{
    public GetStoreVisitByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}