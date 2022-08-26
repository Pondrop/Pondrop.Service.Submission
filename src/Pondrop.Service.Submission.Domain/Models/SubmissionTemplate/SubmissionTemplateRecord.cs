﻿namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record SubmissionTemplateRecord(
        Guid Id,
        string Title,
        string Description,
        int IconCodePoint,
        string IconFontFamily,
        List<StepRecord> Steps,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public SubmissionTemplateRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        new List<StepRecord>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}