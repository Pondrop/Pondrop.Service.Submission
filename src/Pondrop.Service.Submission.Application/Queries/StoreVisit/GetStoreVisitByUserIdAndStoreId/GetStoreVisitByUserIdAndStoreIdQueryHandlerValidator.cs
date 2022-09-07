using FluentValidation;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitByUserIdAndStoreId;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetStoreVisitByUserIdAndStoreIdQueryHandlerValidator : AbstractValidator<GetStoreVisitByUserIdAndStoreIdQuery>
{
    public GetStoreVisitByUserIdAndStoreIdQueryHandlerValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
    }
}