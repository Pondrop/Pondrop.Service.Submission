using Pondrop.Service.Submission.Domain.Enums.Campaign;
using Pondrop.Service.Submission.Domain.Events.Campaign;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Campaign.Domain.Tests;

public class CampaignEntityTests
{
    private string Name = "Test Name";
    private CampaignType CampaignType = CampaignType.task;
    private CampaignStatus CampaignStatus = CampaignStatus.live;
    private List<Guid> CampaignTriggerIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private List<Guid> CampaignFocusCategoryIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private List<Guid> CampaignFocusProductIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private List<Guid> SelectedTemplateIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private List<Guid> StoreIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private DateTime CampaignPublishedDate = DateTime.Now;
    private DateTime CampaignEndDate = DateTime.Now;
    private string PublicationlifecycleId = "1";
    private Guid RewardSchemeId = Guid.NewGuid();
    private int RequiredSubmissions = 2;
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";


    [Fact]
    public void Campaign_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new CampaignEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void Campaign_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewCampaign();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(CampaignType, entity.CampaignType);
        Assert.Equal(CampaignTriggerIds, entity.CampaignTriggerIds);
        Assert.Equal(CampaignFocusCategoryIds, entity.CampaignFocusCategoryIds);
        Assert.Equal(CampaignFocusProductIds, entity.CampaignFocusProductIds);
        Assert.Equal(SelectedTemplateIds, entity.SelectedTemplateIds);
        Assert.Equal(StoreIds, entity.StoreIds);
        Assert.Equal(RewardSchemeId, entity.RewardSchemeId);
        Assert.Equal(RequiredSubmissions, entity.RequiredSubmissions);
        Assert.Equal(CampaignStatus, entity.CampaignStatus);
        Assert.Equal(CampaignPublishedDate, entity.CampaignPublishedDate);
        Assert.Equal(CampaignEndDate, entity.CampaignEndDate);
        Assert.Equal(PublicationlifecycleId, entity.PublicationlifecycleId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    [Fact]
    public void Campaign_UpdateCampaign_ShouldUpdateCampaignToCompleted()
    {
        // arrange
        var entity = GetNewCampaign();
        var updateEvent = GetUpdateCampaign(entity.Id);

        // act
        entity.Apply(updateEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Id, entity.Id);
        Assert.Equal(updateEvent.Name, entity.Name);
        Assert.Equal(updateEvent.CampaignType, entity.CampaignType);
        Assert.Equal(updateEvent.CampaignTriggerIds, entity.CampaignTriggerIds);
        Assert.Equal(updateEvent.CampaignFocusCategoryIds, entity.CampaignFocusCategoryIds);
        Assert.Equal(updateEvent.CampaignFocusProductIds, entity.CampaignFocusProductIds);
        Assert.Equal(updateEvent.SelectedTemplateIds, entity.SelectedTemplateIds);
        Assert.Equal(updateEvent.StoreIds, entity.StoreIds);
        Assert.Equal(updateEvent.RewardSchemeId, entity.RewardSchemeId);
        Assert.Equal(updateEvent.RequiredSubmissions, entity.RequiredSubmissions);
        Assert.Equal(updateEvent.CampaignStatus, entity.CampaignStatus);
        Assert.Equal(updateEvent.CampaignPublishedDate, entity.CampaignPublishedDate);
        Assert.Equal(updateEvent.CampaignEndDate, entity.CampaignEndDate);
        Assert.Equal(updateEvent.PublicationlifecycleId, entity.PublicationlifecycleId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }



    private CampaignEntity GetNewCampaign() => new CampaignEntity(
        Name,
        CampaignType,
        CampaignTriggerIds,
        CampaignFocusCategoryIds,
        CampaignFocusProductIds,
        SelectedTemplateIds,
        StoreIds,
        RequiredSubmissions,
        RewardSchemeId,
        CampaignPublishedDate,
        CampaignEndDate,
        CampaignStatus,
        PublicationlifecycleId,
        CreatedBy);

    private UpdateCampaign GetUpdateCampaign(Guid campaignId) => new UpdateCampaign(
        campaignId,
        Name,
        CampaignType,
        CampaignTriggerIds,
        CampaignFocusCategoryIds,
        CampaignFocusProductIds,
        SelectedTemplateIds,
        StoreIds,
        RequiredSubmissions,
        RewardSchemeId,
        CampaignPublishedDate,
        CampaignEndDate,
        CampaignStatus,
        PublicationlifecycleId);
}