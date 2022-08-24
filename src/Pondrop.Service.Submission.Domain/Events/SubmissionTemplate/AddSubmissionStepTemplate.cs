using Pondrop.Service.Submission.Domain.Events;

public record AddSubmissionStepTemplate(
    Guid Id,
    Guid SubmissionTemplateId,
    string Title,
    string Type) : EventPayload;
