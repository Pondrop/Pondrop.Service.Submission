using FluentValidation;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetSubmissionTemplateById;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionTemplateByIdQueryHandlerValidator : AbstractValidator<GetSubmissionTemplateByIdQuery>
{
    public GetSubmissionTemplateByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}