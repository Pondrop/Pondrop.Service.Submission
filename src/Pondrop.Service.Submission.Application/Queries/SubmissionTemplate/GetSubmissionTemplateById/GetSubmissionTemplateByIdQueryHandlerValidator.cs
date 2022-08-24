using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionTemplateByIdQueryHandlerValidator : AbstractValidator<GetSubmissionTemplateByIdQuery>
{
    public GetSubmissionTemplateByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}