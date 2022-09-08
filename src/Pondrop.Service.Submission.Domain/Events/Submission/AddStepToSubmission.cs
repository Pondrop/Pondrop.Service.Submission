using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Domain.Events.Submission;
public record AddStepToSubmission(
    Guid Id,
    Guid TemplateStepId,
    double Latitude,
    double Longitude,
    DateTime? Started,
    List<SubmissionFieldRecord> Fields) : EventPayload;
