using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.Campaign;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateCampaign;

public class CreateCampaignCommand : IRequest<Result<CampaignRecord>>
{
    public string Name { get; init; } = string.Empty;

    public CampaignType? CampaignType { get; init; } = null;

    public List<Guid>? CampaignTriggerIds { get; init; } = null;

    public List<Guid>? CampaignFocusCategoryIds { get; init; } = null;

    public List<Guid>? CampaignFocusProductIds { get; init; } = null;

    public List<Guid>? SelectedTemplateIds { get; init; } = null;

    public List<Guid>? StoreIds { get; init; } = null;

    public int RequiredSubmissions { get; init; } = 0;

    public Guid? RewardSchemeId { get; init; } = null;

    public DateTime? CampaignPublishedDate { get; init; } = null;

    public DateTime? CampaignEndDate { get; init; } = null;

    public DateTime? CampaignStartDate { get; init; } = null;

    public int? MinimumTimeIntervalMins { get; init; } = null;

    public int? RepeatEvery { get; init; } = null;

    public string? RepeatEveryUOM { get; init; } = string.Empty;

    public CampaignStatus? CampaignStatus { get; init; } = null;

    public string PublicationlifecycleId { get; init; } = string.Empty;
}