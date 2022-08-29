using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;

public record RemoveStep(
    Guid Id,
    Guid SubmissionTemplateId) : EventPayload;
