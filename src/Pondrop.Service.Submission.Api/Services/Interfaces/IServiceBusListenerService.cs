using Pondrop.Service.Submission.Application.Commands;
using System.Collections.Concurrent;

namespace Pondrop.Service.Submission.Api.Services;

public interface IServiceBusListenerService
{
    Task StartListener();

    Task StopListener();

    ValueTask DisposeAsync();
}