using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionViewRecord(
        Guid Id,
        Guid StoreVisitId,
        Guid SubmissionTemplateId,
        SubmissionTemplateRecord SubmissionTemplate,
        string? StoreName,
        string? RetailerName,
        double Latitude,
        double Longitude,
        List<SubmissionStepRecord> Steps,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public SubmissionViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        new SubmissionTemplateRecord(),
        string.Empty,
        string.Empty,
        0,
        0,
        new List<SubmissionStepRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}
