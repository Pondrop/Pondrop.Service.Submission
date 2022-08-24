using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Domain.Events.Submission;

public record CreateSubmissionTemplate(
    Guid Id,
    string Title,
    string Description,
    string Icon) : EventPayload;
