using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Submission.Domain.Tests;

public class SubmissionTemplateEntityTests
{
    private const string Title = "Title";
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
        var entity = new SubmissionTemplateEntity();

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
        Assert.Equal(Title, entity.Title);
        Assert.Equal(Description, entity.Description);
        Assert.Equal(IconFontFamily, entity.IconFontFamily);
        Assert.Equal(IconCodePoint, entity.IconCodePoint);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    [Fact]
    public void Submission_AddStep_ShouldAddStep()
    {
        // arrange
        var entity = GetNewSubmission();
        var addEvent = GetAddStep(entity.Id);

        // act
        entity.Apply(addEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Steps.Single().Id);
        Assert.Equal(addEvent.Title, entity.Steps.Single().Title);
        Assert.Equal(addEvent.Instructions, entity.Steps.Single().Instructions);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }


    [Fact]
    public void Submission_RemoveStep_ShouldRemoveStep()
    {
        // arrange
        var entity = GetNewSubmission();
        var addEvent = GetAddStep(entity.Id);
        var removeEvent = new RemoveStepFromSubmissionTemplate(addEvent.Id, entity.Id);
        entity.Apply(addEvent, UpdatedBy);

        // act
        entity.Apply(removeEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Empty(entity.Steps);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(3, entity.EventsCount);
    }

    private SubmissionTemplateEntity GetNewSubmission() => new SubmissionTemplateEntity(
        Title,
        Description,
        IconCodePoint,
        IconFontFamily,
        CreatedBy);

    private AddStepToSubmissionTemplate GetAddStep(Guid storeId) => new AddStepToSubmissionTemplate(
        Guid.NewGuid(),
        storeId,
        Guid.NewGuid().ToString(),
        nameof(AddStepToSubmissionTemplate.Instructions),
        nameof(AddStepToSubmissionTemplate.InstructionsContinueButton),
        nameof(AddStepToSubmissionTemplate.InstructionsSkipButton),
        1213,
        nameof(AddStepToSubmissionTemplate.InstructionsIconFontFamily),
        false,
        new List<Guid>(),
        nameof(CreatedBy),
        nameof(UpdatedBy)
    );
}