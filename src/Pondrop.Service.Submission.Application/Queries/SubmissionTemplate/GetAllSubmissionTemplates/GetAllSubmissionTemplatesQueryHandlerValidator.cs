using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetAllSubmissionTemplatesQueryHandlerValidator : AbstractValidator<GetAllSubmissionTemplatesQuery>
{
    public GetAllSubmissionTemplatesQueryHandlerValidator()
    {
    }
}