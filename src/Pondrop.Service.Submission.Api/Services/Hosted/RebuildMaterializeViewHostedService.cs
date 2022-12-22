using MediatR;
using Pondrop.Service.Submission.Application.Commands;

namespace Pondrop.Service.Submission.Api.Services;

public class RebuildMaterializeViewHostedService : BackgroundService
{
    private readonly IRebuildCheckpointQueueService _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RebuildMaterializeViewHostedService> _logger;

    public RebuildMaterializeViewHostedService(
        IRebuildCheckpointQueueService queue,
        IServiceProvider serviceProvider,
        ILogger<RebuildMaterializeViewHostedService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var command = await _queue.DequeueAsync(stoppingToken);

            try
            {
                var mediator = _serviceProvider.GetService<IMediator>();
                await mediator!.Send(command, stoppingToken);

                switch (command)
                {
                    case RebuildSubmissionCheckpointCommand store:
                        await mediator!.Send(new RebuildSubmissionViewCommand(), stoppingToken);
                        await mediator!.Send(new RebuildSubmissionWithStoreViewCommand(), stoppingToken);
                        await mediator!.Send(new RebuildFocusedProductSubmissionViewCommand(), stoppingToken);
                        break;
                    case RebuildSubmissionTemplateCheckpointCommand submissionTemplate:
                        await mediator!.Send(new RebuildSubmissionTemplateViewCommand(), stoppingToken);
                        break;
                    case RebuildCampaignCheckpointCommand campaign:
                        await mediator!.Send(new RebuildCampaignViewCommand(), stoppingToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run rebuild materialize view {command.GetType().Name}");
            }
        }
    }
}