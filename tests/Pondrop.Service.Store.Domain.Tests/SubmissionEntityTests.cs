using Pondrop.Service.Submission.Domain.Events.Submission;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Submission.Domain.Tests;

public class SubmissionEntityTests
{
    private Guid StoreVisitId = Guid.NewGuid();
    private Guid SubmissionTemplateId = Guid.NewGuid();
    private Guid CampaignId = Guid.NewGuid();
    private double Latitude = 12.2323;
    private double Longitude = 13.4;
    private const string Description = "Description";
    private const string IconFontFamily = "MaterialIcons";
    private const int IconCodePoint = 13131;
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";


    [Fact]
    public void Submission_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new SubmissionEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void Submission_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewSubmission();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(StoreVisitId, entity.StoreVisitId);
        Assert.Equal(SubmissionTemplateId, entity.SubmissionTemplateId);
        Assert.Equal(Latitude, entity.Latitude);
        Assert.Equal(Longitude, entity.Longitude);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    [Fact]
    public void Submission_AddStep_ShouldAddStepToSubmission()
    {
        // arrange
        var entity = GetNewSubmission();
        var addEvent = GetAddStep(entity.Id);

        // act
        entity.Apply(addEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Steps.Single().Id);
        Assert.Equal(addEvent.TemplateStepId, entity.Steps.Single().TemplateStepId);
        Assert.Equal(addEvent.Latitude, entity.Steps.Single().Latitude);
        Assert.Equal(addEvent.Longitude, entity.Steps.Single().Longitude);
        Assert.Equal(addEvent.Started, DateTime.MinValue);
        Assert.Equal(addEvent.Fields, entity.Steps.Single().Fields);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }



    private SubmissionEntity GetNewSubmission() => new SubmissionEntity(
        StoreVisitId,
        SubmissionTemplateId,
        CampaignId,
        Latitude,
        Longitude,
        CreatedBy);

    private AddStepToSubmission GetAddStep(Guid storeId) => new AddStepToSubmission(
        StoreVisitId,
        SubmissionTemplateId,
        Latitude,
        Longitude,
        DateTime.MinValue,
        new List<SubmissionFieldRecord>()
    );
}