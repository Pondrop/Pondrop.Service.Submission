namespace Pondrop.Service.Submission.Domain.Models.Submission;

public record FieldValuesRecord(
    Guid Id,
    string SelectedStringValue,
    double SelectedDoubleValue,
    bool SelectedBoolValue)
{
    public FieldValuesRecord() : this(
        Guid.NewGuid(),
        string.Empty,
        0,
        false)
    {
    }
}