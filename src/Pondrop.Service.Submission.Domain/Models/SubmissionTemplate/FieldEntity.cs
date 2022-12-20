using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Events.Field;

namespace Pondrop.Service.Submission.Domain.Models;

public record FieldEntity : EventEntity
{
    public FieldEntity()
    {
        Id = Guid.Empty;
        Label = string.Empty;
        Mandatory = false;
        FieldType = SubmissionFieldType.unknown;
        ItemType = SubmissionFieldItemType.unknown;
        FieldStatus = SubmissionFieldStatus.unknown;
        MaxValue = null;
        PickerValues = null;
    }
    public FieldEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public FieldEntity(string label,
    bool mandatory,
    SubmissionFieldStatus fieldStatus,
    SubmissionFieldType fieldType,
    SubmissionFieldItemType? itemType,
    int? maxValue,
    List<string?>? pickerValues, string createdBy) : this()
    {
        var create = new CreateField(Guid.NewGuid(), label, mandatory, fieldStatus, fieldType, itemType, maxValue, pickerValues);
        Apply(create, createdBy);
    }

    [JsonProperty("label")]
    public string Label { get; private set; }

    [JsonProperty("mandatory")]
    public bool Mandatory { get; private set; }

    [JsonProperty("fieldStatus")]
    public SubmissionFieldStatus FieldStatus { get; private set; }


    [JsonProperty("fieldType")]
    public SubmissionFieldType FieldType { get; private set; }

    [JsonProperty("itemType")]
    public SubmissionFieldItemType? ItemType { get; private set; }

    [JsonProperty("maxValue")]
    public int? MaxValue { get; private set; }

    [JsonProperty("pickerValues")]
    public List<string?>? PickerValues { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateField create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateField update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateField create)
        {
            Apply(new Event(GetStreamId<FieldEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateField create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Label = create.Label;
        Mandatory = create.Mandatory;
        FieldType = create.FieldType;
        FieldStatus = create.FieldStatus;
        ItemType = create.ItemType;
        MaxValue = create.MaxValue;
        PickerValues = create.PickerValues;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateField update, string createdBy, DateTime createdUtc)
    {
        {
            var oldLabel = Label;
            var oldMandatory = Mandatory;
            var oldFieldType = FieldType;
            var oldFieldStatus = FieldStatus;
            var oldItemType = ItemType;
            var oldMaxValue = MaxValue;
            var oldPickerValues = PickerValues;

            Label = update.Label;
            Mandatory = update.Mandatory;
            FieldStatus = update.FieldStatus;
            FieldType = update.FieldType;
            ItemType = update.ItemType;
            MaxValue = update.MaxValue;
            PickerValues = update.PickerValues;

            if (oldLabel != Label ||
                oldMandatory != Mandatory ||
                oldFieldType != FieldType ||
                oldFieldStatus != FieldStatus ||
                oldItemType != ItemType ||
                oldMaxValue != MaxValue ||
                oldPickerValues != PickerValues)
            {
                UpdatedBy = createdBy;
                UpdatedUtc = createdUtc;
            }
        }
    }
}