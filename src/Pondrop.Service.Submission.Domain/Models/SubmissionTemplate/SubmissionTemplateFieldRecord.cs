namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record SubmissionTemplateFieldRecord(
        Guid Id,
        string? Label,
        bool? Mandatory,
        int? MaxValue)
{
    public SubmissionTemplateFieldRecord() : this(
        Guid.NewGuid(),
        null,
        null,
        null)
    {
    }
}