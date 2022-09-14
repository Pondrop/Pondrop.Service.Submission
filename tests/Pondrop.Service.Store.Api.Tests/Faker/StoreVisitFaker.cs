using Bogus;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateStoreVisit;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using System;
using System.Collections.Generic;

namespace Pondrop.Service.Submission.Api.Tests.Faker;

public static class StoreVisitFaker
{
    private static readonly string[] Titles = new[] { "Low Stocked Item", "Product Description", "Test" };
    private static readonly string[] Descriptions = new[] { "Test", "Test1", "Test2", };
    private static readonly string[] FieldTypes = new[] { "Picker", "Text", "Photo", };
    private static readonly string[] IconFontFamilies = new[] { "MaterialIcon1", "MaterialIcon2", "MaterialIcon3", };
    private static readonly string[] Instructions = new[] { "Instruction1", "Instruction2", "Instruction3", };
    private static readonly string[] InstructionButtons = new[] { "Okay", "Close", "Accept", };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };

    public static List<StoreVisitRecord> GetStoreVisitRecords(int count = 5)
    {
        var faker = new Faker<StoreVisitRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.UserId, f => Guid.NewGuid())
            .RuleFor(x => x.StoreId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static StoreVisitRecord GetStoreVisitRecord(CreateStoreVisitCommand command)
    {
        var faker = new Faker<StoreVisitRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.UserId, f => Guid.NewGuid())
            .RuleFor(x => x.StoreId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }

    public static CreateStoreVisitCommand GetCreateStoreVisitCommand()
    {
        var faker = new Faker<CreateStoreVisitCommand>()
            .RuleFor(x => x.StoreId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double());

        return faker.Generate();
    }
}