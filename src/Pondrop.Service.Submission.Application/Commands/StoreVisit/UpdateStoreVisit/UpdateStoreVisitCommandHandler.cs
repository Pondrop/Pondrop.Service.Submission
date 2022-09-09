using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using Pondrop.Service.Submission.Domain.Events.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Commands;

public class UpdateStoreVisitCommandHandler : DirtyCommandHandler<StoreVisitEntity, UpdateStoreVisitCommand, Result<StoreVisitRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<StoreVisitEntity> _storeVisitCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateStoreVisitCommand> _validator;
    private readonly ILogger<UpdateStoreVisitCommandHandler> _logger;

    public UpdateStoreVisitCommandHandler(
        IOptions<SubmissionUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateStoreVisitCommand> validator,
        ILogger<UpdateStoreVisitCommandHandler> logger) : base(eventRepository, storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _storeVisitCheckpointRepository = storeVisitCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreVisitRecord>> Handle(UpdateStoreVisitCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreVisitRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreVisitRecord>);

        try
        {
            var storeEntity = await _storeVisitCheckpointRepository.GetByIdAsync(command.Id);
            storeEntity ??= await GetFromStreamAsync(command.Id);

            if (storeEntity is not null)
            {
                var evtPayload = new UpdateStoreVisit(
                    command.Id,
                    command.Latitude ?? 0,
                    command.Longitude ?? 0,
                    command.ShopModeStatus ?? ShopModeStatus.Started);

                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(storeEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _storeVisitCheckpointRepository.FastForwardAsync(storeEntity);
                    success = await UpdateStreamAsync(storeEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(storeEntity.Id, storeEntity.GetEvents(storeEntity.AtSequence)));

                result = success
                    ? Result<StoreVisitRecord>.Success(_mapper.Map<StoreVisitRecord>(storeEntity))
                    : Result<StoreVisitRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<StoreVisitRecord>.Error($"StoreVisit does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<StoreVisitRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateStoreVisitCommand command) =>
        $"Failed to update store\nCommand: '{JsonConvert.SerializeObject(command)}'";
}