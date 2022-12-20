using Pondrop.Service.Events;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Events.Field;
public record UpdateField(
    Guid Id,
    string Label,
    bool Mandatory,
    SubmissionFieldStatus FieldStatus,
    SubmissionFieldType FieldType,
    SubmissionFieldItemType? ItemType,
    int? MaxValue,
    List<string?>? PickerValues) : EventPayload;

       