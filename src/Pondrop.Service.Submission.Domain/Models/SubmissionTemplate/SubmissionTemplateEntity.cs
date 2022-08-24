using Newtonsoft.Json;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Events.Submission;

namespace Pondrop.Service.Submission.Domain.Models;

public record SubmissionTemplateEntity : EventEntity
{
    public SubmissionTemplateEntity()
    {
        Id = Guid.Empty;
        Title = string.Empty;
        Icon = string.Empty;
        Description = string.Empty;
        StepTemplates = new List<SubmissionStepTemplateRecord>();
    }

    public SubmissionTemplateEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public SubmissionTemplateEntity(string title, string description, string icon, string createdBy) : this()
    {
        var create = new CreateSubmissionTemplate(Guid.NewGuid(), title, description, icon);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "Title")]
    public string Title { get; private set; }

    [JsonProperty(PropertyName = "Description")]
    public string Description { get; private set; }

    [JsonProperty(PropertyName = "Icon")]
    public string Icon { get; private set; }

    [JsonProperty(PropertyName = "stepTemplates")]
    public List<SubmissionStepTemplateRecord> StepTemplates { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateSubmissionTemplate create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case AddSubmissionStepTemplate addAddress:
                When(addAddress, eventToApply.CreatedBy, eventToApply.CreatedUtc);
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
        Icon = create.Icon;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(AddSubmissionStepTemplate stepTemplate, string createdBy, DateTime createdUtc)
    {
        StepTemplates.Add(new SubmissionStepTemplateRecord(
            stepTemplate.Id,
            stepTemplate.Title,
            stepTemplate.Type,
            createdBy,
            createdBy,
            createdUtc,
            createdUtc));

        UpdatedBy = createdBy;
        UpdatedUtc = createdUtc;
    }
}