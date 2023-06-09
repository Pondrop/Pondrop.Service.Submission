﻿using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionViewRecord(
        Guid Id,
        Guid StoreVisitId,
        Guid SubmissionTemplateId,
        Guid StoreId,
        string? TemplateTitle,
        DateTime SubmittedUtc,
        string? StoreName,
        string? RetailerName,
        double Latitude,
        double Longitude,
        List<SubmissionStepWithDetailsViewRecord> Steps,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public SubmissionViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        DateTime.MinValue,
        string.Empty,
        string.Empty,
        0,
        0,
        new List<SubmissionStepWithDetailsViewRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}
