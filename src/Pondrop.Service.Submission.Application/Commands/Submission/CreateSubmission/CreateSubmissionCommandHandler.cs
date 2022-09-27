using AutoMapper;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.BlobStorage;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events.Submission;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;

public class CreateSubmissionCommandHandler : DirtyCommandHandler<SubmissionEntity, CreateSubmissionCommand, Result<SubmissionRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateSubmissionCommand> _validator;
    private readonly ILogger<CreateSubmissionCommandHandler> _logger;
    private readonly IBlobStorageService _blobStorageService;

    public CreateSubmissionCommandHandler(
        IOptions<SubmissionUpdateConfiguration> SubmissionUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IBlobStorageService blobStorageService,
        IValidator<CreateSubmissionCommand> validator,
        ILogger<CreateSubmissionCommandHandler> logger) : base(eventRepository, SubmissionUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SubmissionRecord>> Handle(CreateSubmissionCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create submission template failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionRecord>.Error(errorMessage);
        }

        var result = default(Result<SubmissionRecord>);
        

        try
        {
            var SubmissionEntity = new SubmissionEntity(
                command.StoreVisitId,
                command.SubmissionTemplateId,
                command.Latitude ?? 0,
                command.Longitude ?? 0,
                _userService.CurrentUserId()
               );

            foreach (var step in command.Steps)
            {
                if (step == null)
                    continue;

                var stepFields = new List<SubmissionFieldRecord>();

                foreach (var field in step.Fields)
                {
                    var fieldValueRecords = new List<FieldValuesRecord>();

                    if (field is null)
                        continue;

                    foreach (var fieldValue in field.Values)
                    {
                        string? url = null;

                        if (fieldValue is null)
                            continue;

                        if (!string.IsNullOrEmpty(fieldValue.PhotoBase64) &&
                            !string.IsNullOrEmpty(fieldValue.PhotoFileName))
                        {
                            url = await _blobStorageService.UploadImageAsync(fieldValue.PhotoFileName,
                                fieldValue.PhotoBase64,
                                _userService.CurrentUserId());
                        }

                        fieldValueRecords.Add(new FieldValuesRecord(fieldValue.Id,
                            fieldValue.StringValue,
                            fieldValue.IntValue,
                            fieldValue.DoubleValue,
                            url,
                            !string.IsNullOrEmpty(fieldValue.ItemValue?.ItemId)
                            ? new ItemValueRecord(
                                fieldValue.ItemValue.ItemId,
                                fieldValue.ItemValue.ItemName,
                                fieldValue.ItemValue.ItemType)
                            : null));
                    }

                    stepFields.Add(new SubmissionFieldRecord(field.Id, field.TemplateFieldId, field.Latitude ?? 0, field.Longitude ?? 0, fieldValueRecords));
                }

                var addStepToSubmission = new Domain.Events.Submission.AddStepToSubmission(
                        Guid.NewGuid(),
                        step!.TemplateStepId,
                        step!.Latitude ?? 0,
                        step!.Longitude ?? 0,
                        step!.StartedUtc,
                        stepFields);

                SubmissionEntity.Apply(addStepToSubmission, _userService.CurrentUserId());
            }
            var success = await _eventRepository.AppendEventsAsync(SubmissionEntity.StreamId, 0, SubmissionEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(SubmissionEntity.Id, SubmissionEntity.GetEvents()));

            result = success
                ? Result<SubmissionRecord>.Success(_mapper.Map<SubmissionRecord>(SubmissionEntity))
                : Result<SubmissionRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SubmissionRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateSubmissionCommand command) =>
        $"Failed to create submission template \nCommand: '{JsonConvert.SerializeObject(command)}'";
}