using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateStoreVisitCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateStoreVisitCheckpointByIdCommand, StoreVisitEntity, StoreVisitRecord>
{
    public UpdateStoreVisitCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateStoreVisitCheckpointByIdCommandHandler> logger) : base(eventRepository, storeVisitCheckpointRepository, mapper, validator, logger)
    {
    }
}