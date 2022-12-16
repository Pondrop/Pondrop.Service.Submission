using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetAllSubmissionTemplates;

public class GetAllActiveSubmissionTemplatesQuery : IRequest<Result<List<SubmissionTemplateViewRecord>>>
{
    public int Offset { get; set; }

    public int Limit { get; set; }

}