using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

public record FieldRecord(
        Guid Id,
        string Label,
        bool Mandatory,
        SubmissionFieldStatus FieldStatus,
        SubmissionFieldType FieldType,
        SubmissionTemplateType TemplateType,
        SubmissionFieldItemType? ItemType,
        int? MaxValue,
        List<string?>? PickerValues)
{
    public FieldRecord() : this(
        Guid.NewGuid(),
        string.Empty,
        false,
        SubmissionFieldStatus.unknown,
        SubmissionFieldType.unknown,
        SubmissionTemplateType.all,
        null,
        null,
        null)
    {
    }
}