using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.Field.GetFieldById;

public class GetFieldByIdQuery : IRequest<Result<FieldRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}