using Pondrop.Service.Submission.Domain.Events;

namespace Pondrop.Service.Submission.Application.Interfaces;

public interface IServiceBusService
{
    Task SendMessageAsync(object payload);

    Task SendMessageAsync(string subject, object payload);
}