using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.Submission;

public record SubmissionStepViewRecord(
        Guid Id,
        Guid TemplateStepId, 
        double Latitude,
        double Longitude,
        List<SubmissionFieldRecord> Fields)
{
    public SubmissionStepViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        new List<SubmissionFieldRecord>())
    {
    }
}