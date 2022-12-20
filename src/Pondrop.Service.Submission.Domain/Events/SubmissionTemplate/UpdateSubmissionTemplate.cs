using Pondrop.Service.Events;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record UpdateSubmissionTemplate(
    Guid Id,
    string Title,
    string Description,
    int IconCodePoint,
    string IconFontFamily,
    SubmissionTemplateType Type,
    SubmissionTemplateStatus Status,
    bool? IsForManualSubmissions,
    SubmissionTemplateFocus Focus) : EventPayload;
