using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.Field.GetAllFields;

public class GetAllFieldsQuery : IRequest<Result<List<FieldRecord>>>
{
    public int Limit { get; set; }

    public int Offset { get; set; }
}