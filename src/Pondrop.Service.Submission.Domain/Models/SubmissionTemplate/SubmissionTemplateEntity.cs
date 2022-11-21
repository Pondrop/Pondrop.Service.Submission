﻿using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
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
    }

    public SubmissionTemplateEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public SubmissionTemplateEntity(string title, string description, int iconCodePoint, string iconFontFamily, string createdBy) : this()
    {
        var create = new CreateSubmissionTemplate(Guid.NewGuid(), title, description, iconCodePoint, iconFontFamily);
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

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateSubmissionTemplate create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
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
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(AddStepToSubmissionTemplate step, string createdBy, DateTime createdUtc)
    {
        Steps.Add(new StepRecord(
            step.Id,
            step.Title,
            step.Instructions,
            step.InstructionsContinueButton,
            step.InstructionsSkipButton,
            step.InstructionsIconCodePoint,
            step.InstructionsIconFontFamily,
            step.IsSummary,
            step.FieldIds,
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