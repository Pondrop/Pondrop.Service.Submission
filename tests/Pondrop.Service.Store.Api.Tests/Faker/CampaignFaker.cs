using Bogus;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateCampaign;
using Pondrop.Service.Submission.Domain.Enums.Campaign;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using System;
using System.Collections.Generic;

namespace Pondrop.Service.Submission.Api.Tests.Faker;

public static class CampaignFaker
{
    private static readonly string[] Names = new[] { "Test 1", "Test 2", "Test 3" };
    private static readonly string[] TemplateTitles = new[] { "Title 1", "Title 2", "Title 3" };
    private static readonly CampaignType[] CampaignTypes = new[] { CampaignType.task, CampaignType.orchestration };
    private static readonly CampaignStatus[] CampaignStatuses = new[] { CampaignStatus.draft, CampaignStatus.live, CampaignStatus.ended };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };

    public static List<CampaignRecord> GetCampaignRecords(int count = 5)
    {


        var faker = new Faker<CampaignRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.CampaignType, f => f.PickRandom(CampaignTypes))
            .RuleFor(x => x.CampaignTriggerIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusCategoryIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusProductIds, f => GenerateGuids(3))
            .RuleFor(x => x.SelectedTemplateIds, f => GenerateGuids(3))
            .RuleFor(x => x.StoreIds, f => GenerateGuids(3))
            .RuleFor(x => x.RewardSchemeId, f => f.Random.Guid())
            .RuleFor(x => x.RequiredSubmissions, f => f.Random.Int())
            .RuleFor(x => x.CampaignStatus, f => f.PickRandom(CampaignStatuses))
            .RuleFor(x => x.CampaignPublishedDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.CampaignEndDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.PublicationlifecycleId, f => f.Random.Word())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static CampaignRecord GetCampaignRecord(CreateCampaignCommand command)
    {
        var faker = new Faker<CampaignRecord>()
           .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.CampaignType, f => f.PickRandom(CampaignTypes))
            .RuleFor(x => x.CampaignTriggerIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusCategoryIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusProductIds, f => GenerateGuids(3))
            .RuleFor(x => x.SelectedTemplateIds, f => GenerateGuids(3))
            .RuleFor(x => x.StoreIds, f => GenerateGuids(3))
            .RuleFor(x => x.RewardSchemeId, f => f.Random.Guid())
            .RuleFor(x => x.RequiredSubmissions, f => f.Random.Int())
            .RuleFor(x => x.CampaignStatus, f => f.PickRandom(CampaignStatuses))
            .RuleFor(x => x.CampaignPublishedDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.CampaignEndDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.PublicationlifecycleId, f => f.Random.Word())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }

    public static List<CampaignViewRecord> GetCampaignViewRecords(int count = 5)
    {
        var faker = new Faker<CampaignViewRecord>()
           .RuleFor(x => x.Name, f => f.PickRandom(Names))
            //.RuleFor(x => x.CampaignType, f => f.PickRandom(CampaignTypes))
            .RuleFor(x => x.NumberOfStores, f => f.Random.Int())
            .RuleFor(x => x.SelectedTemplateTitle, f => f.PickRandom(TemplateTitles))
            //.RuleFor(x => x.CampaignStatus, f => f.PickRandom(CampaignStatuses))
            .RuleFor(x => x.Completions, f => f.Random.Int())
            .RuleFor(x => x.CampaignPublishedDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)));

        return faker.Generate(Math.Max(0, count));
    }

    public static CampaignEntity GetCampaignEntity()
    {
        var faker = new Faker<CampaignEntity>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.CampaignType, f => f.PickRandom(CampaignTypes))
            .RuleFor(x => x.CampaignTriggerIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusCategoryIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusProductIds, f => GenerateGuids(3))
            .RuleFor(x => x.SelectedTemplateIds, f => GenerateGuids(3))
            .RuleFor(x => x.StoreIds, f => GenerateGuids(3))
            .RuleFor(x => x.RewardSchemeId, f => f.Random.Guid())
            .RuleFor(x => x.RequiredSubmissions, f => f.Random.Int())
            .RuleFor(x => x.CampaignStatus, f => f.PickRandom(CampaignStatuses))
            .RuleFor(x => x.CampaignPublishedDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.CampaignEndDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.PublicationlifecycleId, f => f.Random.Word())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }



    public static CreateCampaignCommand GetCreateCampaignCommand()
    {
        var faker = new Faker<CreateCampaignCommand>()
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.CampaignType, f => f.PickRandom(CampaignTypes))
            .RuleFor(x => x.CampaignTriggerIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusCategoryIds, f => GenerateGuids(3))
            .RuleFor(x => x.CampaignFocusProductIds, f => GenerateGuids(3))
            .RuleFor(x => x.SelectedTemplateIds, f => GenerateGuids(3))
            .RuleFor(x => x.StoreIds, f => GenerateGuids(3))
            .RuleFor(x => x.RewardSchemeId, f => f.Random.Guid())
            .RuleFor(x => x.RequiredSubmissions, f => f.Random.Int())
            .RuleFor(x => x.CampaignStatus, f => f.PickRandom(CampaignStatuses))
            .RuleFor(x => x.CampaignPublishedDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.CampaignEndDate, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.PublicationlifecycleId, f => f.Random.Word());

        return faker.Generate();
    }

    public static List<Guid> GenerateGuids(int count = 1)
    {
        var list = new List<Guid>(count);
        for (int i = 0; i < count; i++) list.Add(Guid.NewGuid());
        return list;
    }
}