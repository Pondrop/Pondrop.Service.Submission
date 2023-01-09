using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Models;

public record SubmissionTemplateEntity : EventEntity
{
    public SubmissionTemplateEntity()
    {
        Id = Guid.Empty;
        Title = string.Empty;
        IconCodePoint = int.MaxValue;
        IconFontFamily = string.Empty;
        Description = string.Empty;
        Steps = new List<StepRecord>();
        Type = SubmissionTemplateType.unknown;
        Status = SubmissionTemplateStatus.unknown;
        Focus = SubmissionTemplateFocus.unknown;
        InitiatedBy = SubmissionTemplateInitiationType.unknown;
        IsForManualSubmissions = null;
    }

    public SubmissionTemplateEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public SubmissionTemplateEntity(string title, string description, int iconCodePoint, string iconFontFamily, SubmissionTemplateType type, SubmissionTemplateStatus status, bool? isForManualSubmissions, SubmissionTemplateFocus focus, SubmissionTemplateInitiationType initiatedBy, string createdBy) : this()
    {
        var create = new CreateSubmissionTemplate(Guid.NewGuid(), title, description, iconCodePoint, iconFontFamily, type, status, isForManualSubmissions, focus, initiatedBy);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "title")]
    public string Title { get; private set; }

    [JsonProperty(PropertyName = "description")]
    public string Description { get; private set; }

    [JsonProperty("iconCodePoint")]
    public int IconCodePoint { get; private set; }

    [JsonProperty("iconFontFamily")]
    public string IconFontFamily { get; private set; }


    [JsonProperty(PropertyName = "steps")]
    public List<StepRecord> Steps { get; private set; }

    [JsonProperty("type")]
    public SubmissionTemplateType Type { get; private set; }

    [JsonProperty("isForManualSubmissions")]
    public bool? IsForManualSubmissions { get; private set; }


    [JsonProperty("status")]
    public SubmissionTemplateStatus Status { get; private set; }

    [JsonProperty("focus")]
    public SubmissionTemplateFocus Focus { get; private set; }

    [JsonProperty("initiatedBy")]
    public SubmissionTemplateInitiationType InitiatedBy { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateSubmissionTemplate create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateSubmissionTemplate update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case AddStepToSubmissionTemplate addStep:
                When(addStep, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case RemoveStepFromSubmissionTemplate removeStep:
                When(removeStep, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateSubmissionTemplate create)
        {
            Apply(new Event(GetStreamId<SubmissionTemplateEntity>(create.Id), StreamType, 0, create, createdBy));

        }
        if (eventPayloadToApply is UpdateSubmissionTemplate update)
        {
            Apply(new Event(GetStreamId<SubmissionTemplateEntity>(update.Id), StreamType, 0, update, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateSubmissionTemplate create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Title = create.Title;
        Description = create.Description;
        IconCodePoint = create.IconCodePoint;
        IconFontFamily = create.IconFontFamily;
        Type = create.Type;
        Status = create.Status;
        IsForManualSubmissions = create.IsForManualSubmissions;
        Focus = create.Focus;
        InitiatedBy = create.InitiatedBy;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateSubmissionTemplate update, string createdBy, DateTime createdUtc)
    {
        var oldTitle = Title;
        var oldDescription = Description;
        var oldIconCodePoint = IconCodePoint;
        var oldIconFontFamily = IconFontFamily;
        var oldType = Type;
        var oldStatus = Status;
        var oldIsForManualSubmissions = IsForManualSubmissions;
        var oldFocus = Focus;
        var oldInitiatedBy = InitiatedBy;

        Title = update.Title;
        Description = update.Description;
        IconCodePoint = update.IconCodePoint;
        IconFontFamily = update.IconFontFamily;
        Type = update.Type;
        Status = update.Status;
        IsForManualSubmissions = update.IsForManualSubmissions;
        InitiatedBy = update.InitiatedBy;
        Focus = update.Focus;

        if (oldTitle != Title ||
            oldDescription != Description ||
            oldIconCodePoint != IconCodePoint ||
            oldIconFontFamily != IconFontFamily ||
            oldType != Type ||
            oldStatus != Status ||
            oldFocus != Focus ||
            oldInitiatedBy != InitiatedBy ||
            oldIsForManualSubmissions != IsForManualSubmissions)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }

    private void When(AddStepToSubmissionTemplate step, string createdBy, DateTime createdUtc)
    {
        Steps.Add(new StepRecord(
            step.Id,
            step.Title,
            step.Instructions,
            step.InstructionsStep,
            step.InstructionsContinueButton,
            step.InstructionsSkipButton,
            step.InstructionsIconCodePoint,
            step.InstructionsIconFontFamily,
            step.IsSummary,
            step.FieldDefinitions,
            createdBy,
            createdBy,
            createdUtc,
            createdUtc,
            null));

        UpdatedBy = createdBy;
        UpdatedUtc = createdUtc;
    }

    private void When(RemoveStepFromSubmissionTemplate removeItemFromList, string updatedBy, DateTime updatedUtc)
    {
        var step = Steps.Single(i => i.Id == removeItemFromList.Id);
        Steps.Remove(step);

        UpdatedBy = updatedBy;
        UpdatedUtc = updatedUtc;
    }
}