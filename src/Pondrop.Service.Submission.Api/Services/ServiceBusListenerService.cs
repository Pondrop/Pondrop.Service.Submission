using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Submission.Application.Commands;
using System.Text;

namespace Pondrop.Service.Submission.Api.Services;

public class ServiceBusListenerService : IServiceBusListenerService
{
    private readonly ILogger<ServiceBusListenerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly ServiceBusConfiguration _config;

    private readonly ServiceBusClient _serviceBusClient;

    private ServiceBusProcessor _processor;

    public ServiceBusListenerService(
        IOptions<ServiceBusConfiguration> config,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<ServiceBusListenerService> logger)
    {
        _mediator = mediator;
        _logger = logger;
        _serviceProvider = serviceProvider;

        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("Service Bus 'ConnectionString' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.QueueName))
            throw new ArgumentException("Service Bus 'QueueName' cannot be null or empty"); ;

        _config = config.Value;

        _serviceBusClient = new ServiceBusClient(_config.ConnectionString);
    }


    public async Task StartListener()
    {
        ServiceBusProcessorOptions _serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
        };

        _processor = _serviceBusClient.CreateProcessor(_config.QueueName, _serviceBusProcessorOptions);
        _processor.ProcessMessageAsync += ProcessMessagesAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;


        await _processor.StartProcessingAsync().ConfigureAwait(false);
    }

    public async Task StopListener()
    {
        await _processor.CloseAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync().ConfigureAwait(false);
        }

        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        try
        {
            if (args.Message.Subject != null && args.Message.Subject.Contains("Command"))
            {
                var payload = Encoding.UTF8.GetString(args.Message.Body);

                var commandType = GetCommandType(args.Message.Subject);

                if (commandType is not null && !string.IsNullOrEmpty(payload))
                {
                    var command = JsonConvert.DeserializeObject<JObject>(payload)?.ToObject(commandType);
                    if (command is not null)
                    {
                        try
                        {
                            using var scoped = _serviceProvider.CreateScope();
                            var mediator = scoped.ServiceProvider.GetService<IMediator>();
                            await mediator!.Send(command);

                            switch (command)
                            {
                                case UpdateSubmissionTemplateCheckpointByIdCommand SubmissionTemplate:
                                    await mediator!.Send(new UpdateSubmissionTemplateViewCommand() { SubmissionTemplateId = SubmissionTemplate.Id });
                                    break;
                                case UpdateSubmissionCheckpointByIdCommand Submission:
                                    //await mediator!.Send(new UpdateSubmissionViewCommand() { SubmissionId = Submission.Id });
                                    await mediator!.Send(new UpdateSubmissionWithStoreViewCommand() { SubmissionId = Submission.Id });
                                    await mediator!.Send(new UpdateCampaignViewCommand() { SubmissionId = Submission.Id });
                                    await mediator!.Send(new UpdateFocusedProductSubmissionViewCommand() { SubmissionId = Submission.Id });
                                    break;
                                case UpdateStoreVisitCheckpointByIdCommand StoreVisit:
                                    //await mediator!.Send(new UpdateSubmissionViewCommand() { StoreVisitId = StoreVisit.Id });
                                    break;
                                case UpdateCampaignCheckpointByIdCommand Campaign:
                                    await mediator!.Send(new UpdateCampaignViewCommand() { CampaignId = Campaign.Id });
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to run process event '{args.Message.Subject}'");
                        }
                    }
                }
            }

            if (args.Message.Body != null)
            {
                var payload = Encoding.UTF8.GetString(args.Message.Body);

                if (!string.IsNullOrEmpty(payload))
                {
                    var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(payload);
                    var eventGridData = eventGridEvent?.Data as JObject;
                    var eventData = eventGridData?.ToObject<UpdateSubmissionWithStoreViewCommand>();

                    using var scoped = _serviceProvider.CreateScope();
                    var mediator = scoped.ServiceProvider.GetService<IMediator>();

                    if (eventData != null)
                    {
                        await mediator!.Send(eventData);
                    }
                }
            }
        }
        finally
        {
            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Message handler encountered an exception");
        _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
        _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
        _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

        return Task.CompletedTask;
    }

    private Type? GetCommandType(string commandString)
    {
        var commandType = typeof(UpdateCheckpointByIdCommand);
        var commandTypeName = $"{commandType.FullName!.Replace(nameof(UpdateCheckpointByIdCommand), commandString)}, {commandType.Assembly.GetName()}";

        commandType = Type.GetType(commandTypeName);
        return commandType;
    }
}