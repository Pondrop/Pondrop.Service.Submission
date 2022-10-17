using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.Submission;

public record FieldValuesRecord(
    Guid Id,
    string? StringValue,
    int? IntValue,
    double? DoubleValue,
    DateTime? DateTimeValue,
    string? PhotoUrl,
    ItemValueRecord? ItemValue)
{
    public FieldValuesRecord() : this(
        Guid.NewGuid(),
        null,
        null,
        null,
        null,
        null,
        null)
    {
    }
}

public record ItemValueRecord(
    string ItemId,
    string ItemName,
    SubmissionFieldItemType ItemType)
{
    public ItemValueRecord() : this(
        string.Empty,
        string.Empty,
        SubmissionFieldItemType.unknown)
    {
    }
}