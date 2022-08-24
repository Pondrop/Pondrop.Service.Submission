using MediatR;
using Pondrop.Service.Submission.Application.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public abstract class RebuildCheckpointCommand : IRequest<Result<int>> 
{
}