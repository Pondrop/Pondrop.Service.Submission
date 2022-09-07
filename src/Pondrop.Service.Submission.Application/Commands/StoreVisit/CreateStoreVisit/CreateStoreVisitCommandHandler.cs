using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateStoreVisit;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Commands.StoreVisit.CreateStoreVisit;

public class CreateStoreVisitCommandHandler : DirtyCommandHandler<StoreVisitEntity, CreateStoreVisitCommand, Result<StoreVisitRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateStoreVisitCommand> _validator;
    private readonly ILogger<CreateStoreVisitCommandHandler> _logger;

    public CreateStoreVisitCommandHandler(
        IOptions<SubmissionUpdateConfiguration> SubmissionUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateStoreVisitCommand> validator,
        ILogger<CreateStoreVisitCommandHandler> logger) : base(eventRepository, SubmissionUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreVisitRecord>> Handle(CreateStoreVisitCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create submission template failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreVisitRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreVisitRecord>);

        var createdBy = _userService.CurrentUserName();

        try
        {
            var storeVisitEntity = new StoreVisitEntity(
                command.StoreId,
                command.UserId,
                command.Latitude,
                command.Longitude,
                ShopModeStatus.Started,
                createdBy
               );

            var success = await _eventRepository.AppendEventsAsync(storeVisitEntity.StreamId, 0, storeVisitEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(storeVisitEntity.Id, storeVisitEntity.GetEvents()));

            result = success
                ? Result<StoreVisitRecord>.Success(_mapper.Map<StoreVisitRecord>(storeVisitEntity))
                : Result<StoreVisitRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<StoreVisitRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateStoreVisitCommand command) =>
        $"Failed to create submission template \nCommand: '{JsonConvert.SerializeObject(command)}'";
}