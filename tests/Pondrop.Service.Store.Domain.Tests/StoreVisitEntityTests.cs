using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using Pondrop.Service.Submission.Domain.Events.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.StoreVisit.Domain.Tests;

public class StoreVisitEntityTests
{
    private Guid StoreId = Guid.NewGuid();
    private Guid StoreVisitId = Guid.NewGuid();
    private Guid UserId = Guid.NewGuid();
    private ShopModeStatus ShopModeStatusStarted = Submission.Domain.Enums.StoreVisit.ShopModeStatus.Started;
    private ShopModeStatus ShopModeStatusCompleted = Submission.Domain.Enums.StoreVisit.ShopModeStatus.Completed;
    private double Latitude = 12.2323;
    private double Longitude = 13.4;
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";


    [Fact]
    public void StoreVisit_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new StoreVisitEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void StoreVisit_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewStoreVisit();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(StoreId, entity.StoreId);
        Assert.Equal(UserId, entity.UserId);
        Assert.Equal(Latitude, entity.Latitude);
        Assert.Equal(Longitude, entity.Longitude);
        Assert.Equal(ShopModeStatusStarted, entity.ShopModeStatus);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    [Fact]
    public void StoreVisit_UpdateStoreVisit_ShouldUpdateStoreVisitToCompleted()
    {
        // arrange
        var entity = GetNewStoreVisit();
        var updateEvent = GetUpdateStoreVisit(entity.Id);

        // act
        entity.Apply(updateEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Id, entity.Id);
        Assert.Equal(updateEvent.Latitude, entity.Latitude);
        Assert.Equal(updateEvent.Longitude, entity.Longitude);
        Assert.Equal(updateEvent.ShopModeStatus, entity.ShopModeStatus);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }



    private StoreVisitEntity GetNewStoreVisit() => new StoreVisitEntity(
        StoreId,
        UserId,
        Latitude,
        Longitude,
        ShopModeStatusStarted,
        CreatedBy);

    private UpdateStoreVisit GetUpdateStoreVisit(Guid storeId) => new UpdateStoreVisit(
        storeId,
        Latitude,
        Longitude,
        ShopModeStatusCompleted);
}