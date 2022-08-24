using Pondrop.Service.Submission.Domain.Events.Submission;
using Pondrop.Service.Submission.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Submission.Domain.Tests;

public class SubmissionTemplateEntityTests
{
    private const string Name = "My Submission";
    private const string Status = "Online";
    private const string ExternalReferenceId = "dc9145d2-b108-482e-ba6e-a141e2fba16f";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";
    
    private const string RetailerName = "My Retailer";
    private const string SubmissionTypeName = "My SubmissionType";
    
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
        Assert.Equal(Name, entity.Name);
        Assert.Equal(Status, entity.Status);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.NotEqual(Guid.Empty, entity.RetailerId);
        Assert.NotEqual(Guid.Empty, entity.SubmissionTypeId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }
    
    [Fact]
    public void Submission_UpdateSubmission_ShouldUpdate()
    {
        // arrange
        var updateEvent = new UpdateSubmission("New Name", null, null, null);
        var entity = GetNewSubmission();
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Name, updateEvent.Name);
        Assert.Equal(Status, entity.Status);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    [Fact]
    public void Submission_AddSubmissionAddress_ShouldAddAddress()
    {
        // arrange
        var entity = GetNewSubmission();
        var addEvent = GetAddSubmissionAddress(entity.Id);
        
        // act
        entity.Apply(addEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Addresses.Single().Id);
        Assert.Equal(addEvent.AddressLine1, entity.Addresses.Single().AddressLine1);
        Assert.Equal(addEvent.AddressLine2, entity.Addresses.Single().AddressLine2);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    [Fact]
    public void Submission_UpdateSubmissionAddress_ShouldUpdateAddress()
    {
        // arrange
        var entity = GetNewSubmission();
        var addEvent = GetAddSubmissionAddress(entity.Id);
        var updateEvent = new UpdateSubmissionAddress(
            addEvent.Id,
            entity.Id,
            addEvent.AddressLine1 + " Updated",
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        entity.Apply(addEvent, UpdatedBy);
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Addresses.Single().Id);
        Assert.Equal(updateEvent.AddressLine1, entity.Addresses.Single().AddressLine1);
        Assert.Equal(addEvent.AddressLine2, entity.Addresses.Single().AddressLine2);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(3, entity.EventsCount);
    }
    
    [Fact]
    public void Submission_RemoveAddressFromSubmission_ShouldRemoveAddress()
    {
        // arrange
        var entity = GetNewSubmission();
        var addEvent = GetAddSubmissionAddress(entity.Id);
        var removeEvent = new RemoveAddressFromSubmission(addEvent.Id, entity.Id);
        entity.Apply(addEvent, UpdatedBy);
        
        // act
        entity.Apply(removeEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Empty(entity.Addresses);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(3, entity.EventsCount);
    }
    
    private SubmissionTemplateEntity GetNewSubmission() => new SubmissionTemplateEntity(
        Name,
        Status,
        ExternalReferenceId,
        GetRetailerRecord().Id,
        GetSubmissionTypeRecord().Id,
        CreatedBy);
    private AddSubmissionAddress GetAddSubmissionAddress(Guid storeId) => new AddSubmissionAddress(
        Guid.NewGuid(), 
        storeId,
        Guid.NewGuid().ToString(), 
        nameof(AddSubmissionAddress.AddressLine1), 
        nameof(AddSubmissionAddress.AddressLine2), 
        nameof(AddSubmissionAddress.Suburb), 
        nameof(AddSubmissionAddress.State), 
        nameof(AddSubmissionAddress.Postcode),
        nameof(AddSubmissionAddress.Country),
        0,
        0);
    private RetailerRecord GetRetailerRecord() => new RetailerRecord(
        Guid.NewGuid(), 
        Guid.NewGuid().ToString(), 
        RetailerName, 
        CreatedBy, 
        UpdatedBy, 
        DateTime.UtcNow.AddDays(-1), 
        DateTime.UtcNow);
    private SubmissionTypeRecord GetSubmissionTypeRecord() => new SubmissionTypeRecord(
        Guid.NewGuid(), 
        Guid.NewGuid().ToString(), 
        SubmissionTypeName, 
        CreatedBy, 
        UpdatedBy, 
        DateTime.UtcNow.AddDays(-1), 
        DateTime.UtcNow);
}