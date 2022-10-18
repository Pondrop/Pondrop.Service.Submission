using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.Campaign;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Campaign.Application.Commands;

public class UpdateCampaignCommand : IRequest<Result<CampaignRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
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

    public CampaignStatus? CampaignStatus { get; init; } = null;

    public string PublicationlifecycleId { get; init; } = string.Empty;
}