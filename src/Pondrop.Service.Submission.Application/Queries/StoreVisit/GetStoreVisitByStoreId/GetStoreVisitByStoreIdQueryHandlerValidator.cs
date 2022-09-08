using FluentValidation;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitByStoreId;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetStoreVisitByStoreIdQueryHandlerValidator : AbstractValidator<GetStoreVisitByStoreIdQuery>
{
    public GetStoreVisitByStoreIdQueryHandlerValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
    }
}