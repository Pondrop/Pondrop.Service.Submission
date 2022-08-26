namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record FieldRecord(
        Guid Id,
        string Label,
        bool Mandatory,
        string FieldType,
        int? MaxValue,
        List<string?>? PickerValues)
{
    public FieldRecord() : this(
        Guid.NewGuid(),
        string.Empty,
        false,
        string.Empty,
        null,
        null)
    {
    }
}