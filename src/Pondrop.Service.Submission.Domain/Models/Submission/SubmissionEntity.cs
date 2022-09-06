using Newtonsoft.Json;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Events.Submission;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionEntity : EventEntity
{
    public SubmissionEntity()
    {
        Id = Guid.Empty;
        StoreVisitId = Guid.Empty;
        SubmissionTemplateId = Guid.Empty;
        Latitude = 0;
        Longitude = 0;
        Steps = new List<SubmissionStepRecord>();
    }

    public SubmissionEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public SubmissionEntity(Guid storeVisitId, Guid submissionTemplateId, double latitude, double longitude, string createdBy) : this()
    {
        var create = new CreateSubmission(Guid.NewGuid(), storeVisitId, submissionTemplateId, latitude, longitude);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "storeVisitId")]
    public Guid StoreVisitId { get; private set; }

    [JsonProperty(PropertyName = "submissionTemplateId")]
    public Guid SubmissionTemplateId { get; private set; }

    [JsonProperty("latitude")]
    public double Latitude { get; private set; }

    [JsonProperty("longitude")]
    public double Longitude { get; private set; }

    [JsonProperty(PropertyName = "steps")]
    public List<SubmissionStepRecord> Steps { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateSubmission create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case AddStepToSubmission addStep:
                When(addStep, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            //case RemoveStepFromSubmissionTemplate removeStep:
            //    When(removeStep, eventToApply.CreatedBy, eventToApply.CreatedUtc);
            //    break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateSubmission create)
        {
            Apply(new Event(GetStreamId<SubmissionEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateSubmission create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        StoreVisitId = create.StoreVisitId;
        SubmissionTemplateId = create.SubmissionTemplateId;
        Latitude = create.Latitude;
        Longitude = create.Longitude;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(AddStepToSubmission step, string createdBy, DateTime createdUtc)
    {
        Steps.Add(new SubmissionStepRecord(
            step.Id,
            step.TemplateStepId,
            step.Latitude,
            step.Longitude,
            step.Fields,
            createdBy,
            createdBy,
            createdUtc,
            createdUtc));

        UpdatedBy = createdBy;
        UpdatedUtc = createdUtc;
    }

    //private void When(RemoveStepFromSubmissionTemplate removeItemFromList, string updatedBy, DateTime updatedUtc)
    //{
    //    var step = Steps.Single(i => i.Id == removeItemFromList.Id);
    //    Steps.Remove(step);

    //    UpdatedBy = updatedBy;
    //    UpdatedUtc = updatedUtc;
    //}
}