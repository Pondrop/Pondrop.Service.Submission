using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.Submission;

public record CreateSubmission(
    Guid Id,
    Guid StoreVisitId,
    Guid SubmissionTemplateId,
    double Latitude,
    double Longitude) : EventPayload;