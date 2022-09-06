using FluentValidation;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionByIdQueryHandlerValidator : AbstractValidator<GetSubmissionByIdQuery>
{
    public GetSubmissionByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}