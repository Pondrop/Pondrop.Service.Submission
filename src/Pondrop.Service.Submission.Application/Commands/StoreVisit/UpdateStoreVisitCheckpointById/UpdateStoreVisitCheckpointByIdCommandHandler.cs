using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

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