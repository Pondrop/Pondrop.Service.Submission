using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetAllSubmissionTemplates;

public class GetAllSubmissionTemplatesQueryHandlerValidator : AbstractValidator<GetAllSubmissionTemplatesQuery>
{
    public GetAllSubmissionTemplatesQueryHandlerValidator()
    {
    }
}