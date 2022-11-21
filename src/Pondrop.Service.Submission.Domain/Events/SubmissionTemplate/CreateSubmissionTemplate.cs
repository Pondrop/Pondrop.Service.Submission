using Pondrop.Service.Events;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record CreateSubmissionTemplate(
    Guid Id,
    string Title,
    string Description,
    int IconCodePoint,
    string IconFontFamily) : EventPayload;
