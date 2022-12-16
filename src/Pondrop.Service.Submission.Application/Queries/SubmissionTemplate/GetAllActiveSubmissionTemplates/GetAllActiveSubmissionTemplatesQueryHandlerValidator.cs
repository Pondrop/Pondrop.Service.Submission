using FluentValidation;

namespace Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetAllSubmissionTemplates;

public class GetAllActiveSubmissionTemplatesQueryHandlerValidator : AbstractValidator<GetAllActiveSubmissionTemplatesQuery>
{
    public GetAllActiveSubmissionTemplatesQueryHandlerValidator()
    {
    }
}