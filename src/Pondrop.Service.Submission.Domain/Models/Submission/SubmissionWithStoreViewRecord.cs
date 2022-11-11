using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionWithStoreViewRecord(
        Guid Id,
        Guid StoreVisitId,
        Guid StoreId,
        Guid? CampaignId,
        Guid SubmissionTemplateId,
        string TaskType,
        DateTime? SubmittedUtc,
        string? RetailerName,
        string? StoreName,
        string Images)
{
    public SubmissionWithStoreViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        null,
        Guid.Empty,
        string.Empty,
        DateTime.MinValue,
        string.Empty,
        string.Empty,
        string.Empty)
    {
    }
}