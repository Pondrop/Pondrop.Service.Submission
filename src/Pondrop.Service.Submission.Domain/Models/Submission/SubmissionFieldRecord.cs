using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionFieldRecord(
    Guid Id,
    Guid TemplateFieldId,
    double Latitude,
    double Longitude,
    List<FieldValuesRecord> Values)
{
    public SubmissionFieldRecord() : this(
        Guid.NewGuid(),
        Guid.NewGuid(),
        0,
        0,
        new List<FieldValuesRecord>())
    {
    }

    public List<object> GetResults(SubmissionFieldType fieldType)
    {
        return Values.Select(i =>
        {
            switch (fieldType)
            {
                case SubmissionFieldType.photo:
                    return (object?)i.PhotoUrl;
                case SubmissionFieldType.text:
                case SubmissionFieldType.multilineText:
                case SubmissionFieldType.picker:
                case SubmissionFieldType.barcode:
                    return i.StringValue;
                case SubmissionFieldType.currency:
                    return i.DoubleValue;
                case SubmissionFieldType.integer:
                    return i.IntValue;
                case SubmissionFieldType.search:
                case SubmissionFieldType.focus:
                    return i.ItemValue;
                case SubmissionFieldType.date:
                    return i.DateTimeValue?.Date;
                default:
                    throw new InvalidOperationException($"Unknown {nameof(SubmissionFieldType)}: '{fieldType}'");
            }
        }).Where(i => i is not null).Cast<object>().ToList();
    }
}