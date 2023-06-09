﻿using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;

public class CreateSubmissionCommand : IRequest<Result<SubmissionRecord>>
{
    public Guid StoreVisitId { get; init; }

    public Guid SubmissionTemplateId { get; set; }

    public Guid? CampaignId { get; set; } = null;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public List<CreateSubmissionStepRecord?> Steps { get; init; } = default;
}

public record CreateSubmissionStepRecord(
    Guid Id,
    Guid TemplateStepId,
    double? Latitude,
    double? Longitude,
    DateTime StartedUtc,
    List<CreateSubmissionFieldRecord> Fields)
{
    public CreateSubmissionStepRecord() : this(
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        DateTime.UtcNow,
        new List<CreateSubmissionFieldRecord>())
    {
    }
}

public record CreateSubmissionFieldRecord(
    Guid Id,
    Guid TemplateFieldId,
    double? Latitude,
    double? Longitude,
    List<CreateFieldValuesRecord> Values)
{
    public CreateSubmissionFieldRecord() : this(
        Guid.NewGuid(),
        Guid.NewGuid(),
        0,
        0,
        new List<CreateFieldValuesRecord>())
    {
    }
}
public record CreateFieldValuesRecord(
    Guid Id,
    string? StringValue,
    int? IntValue,
    double? DoubleValue,
    DateTime? DateTimeValue,
    string? PhotoFileName,
    string? PhotoBase64,
    CreateItemValueRecord? ItemValue)
{
    public CreateFieldValuesRecord() : this(
        Guid.NewGuid(),
        null,
        null,
        null,
        null,
        null,
        null,
        null)
    {
    }
}

public record CreateItemValueRecord(
    string ItemId,
    string ItemName,
    SubmissionFieldItemType ItemType,
    string? ItemBarcode)
{
    public CreateItemValueRecord() : this(
        string.Empty,
        string.Empty,
        SubmissionFieldItemType.unknown,
        null)
    {
    }

    public bool IsValid() =>
        ItemType switch
        {
            SubmissionFieldItemType.category => Guid.TryParse(ItemId, out _) &&
                                                !string.IsNullOrEmpty(ItemName),
            SubmissionFieldItemType.product => (Guid.TryParse(ItemId, out _) || !string.IsNullOrEmpty(ItemBarcode)) &&
                                               !string.IsNullOrEmpty(ItemName),
            SubmissionFieldItemType.unknown => false,
            _ => throw new NotImplementedException($"Cannot validate item type '{ItemType}'")
        };
}
