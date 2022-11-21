using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildCampaignCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildCampaignCheckpointCommand, CampaignEntity>
{
    public RebuildCampaignCheckpointCommandHandler(
        ICheckpointRepository<CampaignEntity> CampaignCheckpointRepository,
        ILogger<RebuildCampaignCheckpointCommandHandler> logger) : base(CampaignCheckpointRepository, logger)
    {
    }
}