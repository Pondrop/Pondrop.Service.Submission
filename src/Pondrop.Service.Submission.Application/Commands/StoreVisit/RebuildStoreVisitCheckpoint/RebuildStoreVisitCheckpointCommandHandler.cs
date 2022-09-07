using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildStoreVisitCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildStoreVisitCheckpointCommand, StoreVisitEntity>
{
    public RebuildStoreVisitCheckpointCommandHandler(
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        ILogger<RebuildStoreVisitCheckpointCommandHandler> logger) : base(storeVisitCheckpointRepository, logger)
    {
    }
}