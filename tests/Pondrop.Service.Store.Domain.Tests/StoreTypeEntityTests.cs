using Pondrop.Service.Submission.Domain.Events.SubmissionType;
using Pondrop.Service.Submission.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Submission.Domain.Tests;

public class SubmissionTypeEntityTests
{
    private const string Name = "My SubmissionType";
    private const string ExternalReferenceId = "dc9145d2-b108-482e-ba6e-a141e2fba16f";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";
    
    [Fact]
    public void SubmissionType_Ctor_ShouldCreateEmpty()
    {
        // arrange
        
        // act
        var entity = new SubmissionTypeEntity();
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }
    
    [Fact]
    public void SubmissionType_Ctor_ShouldCreateEvent()
    {
        // arrange
        
        // act
        var entity = GetNewSubmissionType();
        
        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }
    
    [Fact]
    public void SubmissionType_UpdateSubmissionType_ShouldUpdate()
    {
        // arrange
        var updateEvent = new UpdateSubmissionType("New Name");
        var entity = GetNewSubmissionType();
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Name, updateEvent.Name);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    private SubmissionTypeEntity GetNewSubmissionType() => new SubmissionTypeEntity(Name, ExternalReferenceId, CreatedBy);
}