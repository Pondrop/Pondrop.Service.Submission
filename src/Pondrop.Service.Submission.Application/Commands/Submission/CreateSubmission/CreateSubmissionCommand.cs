using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;

public class CreateSubmissionCommand : IRequest<Result<SubmissionRecord>>
{
    public Guid StoreVisitId { get; init; }

    public Guid SubmissionTemplateId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public List<SubmissionStepRecord?> Steps { get; init; } = default;
}

public record SubmissionStepRecord(
    Guid Id,
    Guid TemplateStepId,
    double Latitude,
    double Longitude,
    List<SubmissionFieldRecord> Fields)
{
    public SubmissionStepRecord() : this(
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        new List<SubmissionFieldRecord>())
    {
    }
}


