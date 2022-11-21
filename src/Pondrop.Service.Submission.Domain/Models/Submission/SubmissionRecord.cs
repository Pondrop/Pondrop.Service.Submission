using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionRecord(
        Guid Id,
        Guid StoreVisitId,
        Guid SubmissionTemplateId,
        Guid? CampaignId,
        double Latitude,
        double Longitude,
        List<SubmissionStepRecord> Steps,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public SubmissionRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        null,
        0,
        0,
        new List<SubmissionStepRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}