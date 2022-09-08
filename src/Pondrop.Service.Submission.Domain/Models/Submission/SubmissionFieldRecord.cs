﻿namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionFieldRecord(
    Guid Id,
    Guid TemplateFieldId,
    double Latitude,
    double Longitude,
    List<FieldValuesRecord> Values)
{
    public SubmissionFieldRecord() : this(
        Guid.NewGuid(),
        Guid.NewGuid(),
        0,
        0,
        new List<FieldValuesRecord>())
    {
    }
}