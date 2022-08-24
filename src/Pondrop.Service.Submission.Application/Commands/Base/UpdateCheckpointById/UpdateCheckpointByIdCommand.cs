using MediatR;

namespace Pondrop.Service.Submission.Application.Commands;

public abstract class UpdateCheckpointByIdCommand
{
    public Guid Id { get; init; } = Guid.Empty;
}

public abstract class UpdateCheckpointByIdCommand<T> : UpdateCheckpointByIdCommand, IRequest<T> 
{
}