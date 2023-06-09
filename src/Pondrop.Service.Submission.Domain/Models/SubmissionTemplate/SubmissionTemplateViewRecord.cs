﻿using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record SubmissionTemplateViewRecord(
         Guid Id,
        string Title,
        string Description,
        int IconCodePoint,
        string IconFontFamily,
        SubmissionTemplateType Type,
        SubmissionTemplateStatus Status,
        bool? IsForManualSubmissions,
        SubmissionTemplateFocus Focus,
        List<StepViewRecord> Steps,
        SubmissionTemplateInitiationType InitiatedBy,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
{
    public SubmissionTemplateViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        int.MinValue,
        string.Empty,
        SubmissionTemplateType.unknown,
        SubmissionTemplateStatus.unknown,
        null,
        SubmissionTemplateFocus.unknown,
        new List<StepViewRecord>(0),
        SubmissionTemplateInitiationType.unknown,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}