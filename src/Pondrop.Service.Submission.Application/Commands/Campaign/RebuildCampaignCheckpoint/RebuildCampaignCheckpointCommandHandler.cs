using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildCampaignCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildCampaignCheckpointCommand, CampaignEntity>
{
    public RebuildCampaignCheckpointCommandHandler(
        ICheckpointRepository<CampaignEntity> CampaignCheckpointRepository,
        ILogger<RebuildCampaignCheckpointCommandHandler> logger) : base(CampaignCheckpointRepository, logger)
    {
    }
}