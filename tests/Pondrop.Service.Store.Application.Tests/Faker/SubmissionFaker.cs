using Bogus;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;
using Pondrop.Service.Submission.Domain.Models.Submission;
using System;
using System.Collections.Generic;

namespace Pondrop.Service.Submission.Api.Tests.Faker;

public static class SubmissionFaker
{
    private static readonly string[] Titles = new[] { "Low Stocked Item", "Product Description", "Test" };
    private static readonly string[] Descriptions = new[] { "Test", "Test1", "Test2", };
    private static readonly string[] FieldTypes = new[] { "Picker", "Text", "Photo", };
    private static readonly string[] IconFontFamilies = new[] { "MaterialIcon1", "MaterialIcon2", "MaterialIcon3", };
    private static readonly string[] Instructions = new[] { "Instruction1", "Instruction2", "Instruction3", };
    private static readonly string[] InstructionButtons = new[] { "Okay", "Close", "Accept", };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };

    public static List<SubmissionRecord> GetSubmissionRecords(int count = 5)
    {
        var faker = new Faker<SubmissionRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.StoreVisitId, f => Guid.NewGuid())
            .RuleFor(x => x.SubmissionTemplateId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.Steps, f => GetStepRecords(1))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<SubmissionStepRecord> GetStepRecords(int count = 1)
    {
        var faker = new Faker<SubmissionStepRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.TemplateStepId, f => Guid.NewGuid())
            .RuleFor(x => x.Fields, f => GetFieldRecords());

        return faker.Generate(Math.Max(0, count));
    }

    public static List<SubmissionFieldRecord> GetFieldRecords(int count = 1)
    {
        var faker = new Faker<SubmissionFieldRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.TemplateFieldId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.Values, GetFieldValuesRecords());
        return faker.Generate(Math.Max(0, count));
    }


    public static List<FieldValuesRecord> GetFieldValuesRecords(int count = 1)
    {
        var faker = new Faker<FieldValuesRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.StringValue, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.DoubleValue, f => f.Random.Double())
            .RuleFor(x => x.IntValue, f => f.Random.Int())
            .RuleFor(x => x.PhotoUrl, string.Empty);
        return faker.Generate(Math.Max(0, count));
    }


    public static CreateSubmissionCommand GetCreateSubmissionCommand()
    {
        var faker = new Faker<CreateSubmissionCommand>()
            .RuleFor(x => x.StoreVisitId, f => Guid.NewGuid())
            .RuleFor(x => x.SubmissionTemplateId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.Steps, f => GetCreateStepRecords(1));

        return faker.Generate();
    }

    public static List<CreateSubmissionStepRecord> GetCreateStepRecords(int count = 1)
    {
        var faker = new Faker<CreateSubmissionStepRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.TemplateStepId, f => Guid.NewGuid())
            .RuleFor(x => x.Fields, f => GetCreateFieldRecords());

        return faker.Generate(Math.Max(0, count));
    }

    public static List<CreateSubmissionFieldRecord> GetCreateFieldRecords(int count = 1)
    {
        var faker = new Faker<CreateSubmissionFieldRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.TemplateFieldId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.Values, GetCreateFieldValuesRecords());
        return faker.Generate(Math.Max(0, count));
    }


    public static List<CreateFieldValuesRecord> GetCreateFieldValuesRecords(int count = 1)
    {
        var faker = new Faker<CreateFieldValuesRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.StringValue, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.DoubleValue, f => f.Random.Double())
            .RuleFor(x => x.IntValue, f => f.Random.Int())
            .RuleFor(x => x.PhotoFileName, string.Empty)
            .RuleFor(x => x.PhotoBase64, string.Empty);
        return faker.Generate(Math.Max(0, count));
    }


    public static SubmissionRecord GetSubmissionRecord(CreateSubmissionCommand command)
    {
        var utcNow = DateTime.UtcNow;

        var faker = new Faker<SubmissionRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.StoreVisitId, f => Guid.NewGuid())
            .RuleFor(x => x.SubmissionTemplateId, f => Guid.NewGuid())
            .RuleFor(x => x.Latitude, f => f.Random.Double())
            .RuleFor(x => x.Longitude, f => f.Random.Double())
            .RuleFor(x => x.Steps, f => GetStepRecords(1))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }
}