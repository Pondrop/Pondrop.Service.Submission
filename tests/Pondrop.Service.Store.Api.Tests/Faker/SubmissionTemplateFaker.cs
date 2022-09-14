using Bogus;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using System;
using System.Collections.Generic;

namespace Pondrop.Service.Submission.Api.Tests.Faker;

public static class SubmissionTemplateFaker
{
    private static readonly string[] Titles = new[] { "Low Stocked Item", "Product Description", "Test" };
    private static readonly string[] Descriptions = new[] { "Test", "Test1", "Test2", };
    private static readonly string[] FieldTypes = new[] { "Picker", "Text", "Photo", };
    private static readonly string[] IconFontFamilies = new[] { "MaterialIcon1", "MaterialIcon2", "MaterialIcon3", };
    private static readonly string[] Instructions = new[] { "Instruction1", "Instruction2", "Instruction3", };
    private static readonly string[] InstructionButtons = new[] { "Okay", "Close", "Accept", };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };

    public static List<SubmissionTemplateRecord> GetSubmissionTemplateRecords(int count = 5)
    {
        var faker = new Faker<SubmissionTemplateRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.PickRandom(Titles))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.IconCodePoint, f => f.Random.Int())
            .RuleFor(x => x.IconFontFamily, f => f.PickRandom(IconFontFamilies))
            .RuleFor(x => x.Steps, f => GetStepRecords(1))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<SubmissionTemplateViewRecord> GetSubmissionTemplateViewRecords(int count = 5)
    {

        var faker = new Faker<SubmissionTemplateViewRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.PickRandom(Titles))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.IconCodePoint, f => f.Random.Int())
            .RuleFor(x => x.IconFontFamily, f => f.PickRandom(IconFontFamilies))
            .RuleFor(x => x.Steps, f => GetStepRecords(1))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<Pondrop.Service.Submission.Domain.Models.SubmissionTemplate.StepRecord> GetStepRecords(int count = 1)
    {
        var faker = new Faker<Pondrop.Service.Submission.Domain.Models.SubmissionTemplate.StepRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.PickRandom(Titles))
            .RuleFor(x => x.Instructions, f => f.PickRandom(Instructions))
            .RuleFor(x => x.InstructionsContinueButton, f => f.PickRandom(InstructionButtons))
            .RuleFor(x => x.InstructionsSkipButton, f => f.PickRandom(InstructionButtons))
            .RuleFor(x => x.InstructionsIconCodePoint, f => f.Random.Int())
            .RuleFor(x => x.InstructionsIconFontFamily, f => f.PickRandom(IconFontFamilies))
            .RuleFor(x => x.Fields, f => GetFieldRecords(1));

        return faker.Generate(Math.Max(0, count));
    }

    public static List<FieldRecord> GetFieldRecords(int count = 1)
    {
        var faker = new Faker<FieldRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.FieldType, f => f.PickRandom(FieldTypes))
            .RuleFor(x => x.MaxValue, f => f.Random.Int())
            .RuleFor(x => x.PickerValues, f => new List<string?>())
            .RuleFor(x => x.Label, f => f.PickRandom(FieldTypes))
            .RuleFor(x => x.Mandatory, f => f.Random.Bool());
        return faker.Generate(Math.Max(0, count));
    }


    public static CreateSubmissionTemplateCommand GetCreateSubmissionTemplateCommand()
    {
        var faker = new Faker<CreateSubmissionTemplateCommand>()
            .RuleFor(x => x.Title, f => f.PickRandom(Titles))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.IconFontFamily, f => f.PickRandom(IconFontFamilies))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.IconCodePoint, f => f.Random.Int());

        return faker.Generate();
    }

    public static AddStepToSubmissionTemplateCommand GetAddStepCommand()
    {
        var faker = new Faker<AddStepToSubmissionTemplateCommand>()
            .RuleFor(x => x.SubmissionId, f => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.PickRandom(Titles))
            .RuleFor(x => x.Instructions, f => f.PickRandom(Instructions))
            .RuleFor(x => x.InstructionsContinueButton, f => f.PickRandom(InstructionButtons))
            .RuleFor(x => x.InstructionsSkipButton, f => f.PickRandom(InstructionButtons))
            .RuleFor(x => x.InstructionsIconCodePoint, f => f.Random.Int())
            .RuleFor(x => x.InstructionsIconFontFamily, f => f.PickRandom(IconFontFamilies))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.Fields, f => GetFieldRecords(1));

        return faker.Generate();
    }

    public static SubmissionTemplateRecord GetSubmissionTemplateRecord(CreateSubmissionTemplateCommand command)
    {
        var utcNow = DateTime.UtcNow;

        var faker = new Faker<SubmissionTemplateViewRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.PickRandom(Titles))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.IconCodePoint, f => f.Random.Int())
            .RuleFor(x => x.IconFontFamily, f => f.PickRandom(IconFontFamilies))
            .RuleFor(x => x.Steps, f => GetStepRecords(1))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }
}