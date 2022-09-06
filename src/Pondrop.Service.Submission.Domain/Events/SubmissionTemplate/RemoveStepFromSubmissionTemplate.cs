using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record RemoveStepFromSubmissionTemplate(
    Guid Id,
    Guid SubmissionTemplateId) : EventPayload;
