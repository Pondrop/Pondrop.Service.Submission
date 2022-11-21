using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildStoreVisitCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildStoreVisitCheckpointCommand, StoreVisitEntity>
{
    public RebuildStoreVisitCheckpointCommandHandler(
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        ILogger<RebuildStoreVisitCheckpointCommandHandler> logger) : base(storeVisitCheckpointRepository, logger)
    {
    }
}