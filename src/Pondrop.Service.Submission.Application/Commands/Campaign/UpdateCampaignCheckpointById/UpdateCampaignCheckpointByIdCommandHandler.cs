using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateCampaignCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateCampaignCheckpointByIdCommand, CampaignEntity, CampaignRecord>
{
    public UpdateCampaignCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<CampaignEntity> campaignCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateCampaignCheckpointByIdCommandHandler> logger) : base(eventRepository, campaignCheckpointRepository, mapper, validator, logger)
    {
    }
}