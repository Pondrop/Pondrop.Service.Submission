namespace Pondrop.Service.Submission.Domain.Models.Submission;

public record FieldValuesRecord(
    Guid Id,
    string? StringValue,
    int? IntValue,
    double? DoubleValue,
    string? PhotoUrl)
{
    public FieldValuesRecord() : this(
        Guid.NewGuid(),
        null,
        null,
        null,
        null)
    {
    }
}