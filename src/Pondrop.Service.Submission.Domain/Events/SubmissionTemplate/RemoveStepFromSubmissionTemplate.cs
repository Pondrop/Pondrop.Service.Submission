using Pondrop.Service.Events;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record RemoveStepFromSubmissionTemplate(
    Guid Id,
    Guid SubmissionTemplateId) : EventPayload;
