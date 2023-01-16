using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Events.Field;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.Field.AddStepToSubmission;

public class UpdateFieldCommandHandler : DirtyCommandHandler<FieldEntity, UpdateFieldCommand, Result<FieldRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<FieldEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateFieldCommand> _validator;
    private readonly ILogger<UpdateFieldCommandHandler> _logger;

    public UpdateFieldCommandHandler(
        IOptions<SubmissionUpdateConfiguration> submissionTemplateUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<FieldEntity> checkpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateFieldCommand> validator,
        ILogger<UpdateFieldCommandHandler> logger) : base(eventRepository, submissionTemplateUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<FieldRecord>> Handle(UpdateFieldCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update field failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<FieldRecord>.Error(errorMessage);
        }

        var result = default(Result<FieldRecord>);

        try
        {
            var duplicateMessage = $"Possible field match found";
            var existingFields = await GetExistingFieldByLabelAndId(command.Id, command.Label);
            if (existingFields != null && existingFields.Count > 0)
                return Result<FieldRecord>.Error(duplicateMessage);

            var fieldEntity = await _checkpointRepository.GetByIdAsync(command.Id);
            fieldEntity ??= await GetFromStreamAsync(command.Id);

            if (fieldEntity is not null)
            {
                var evtPayload = new UpdateField(
                    command.Id,
                    command.Label,
                    command!.Mandatory ?? false,
                    command!.FieldStatus ?? SubmissionFieldStatus.unknown,
                    command!.FieldType ?? SubmissionFieldType.unknown,
                    command!.TemplateType ?? SubmissionTemplateType.all,
                    command!.ItemType,
                    command!.MaxValue,
                    command!.PickerValues);

                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(fieldEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _checkpointRepository.FastForwardAsync(fieldEntity);
                    success = await UpdateStreamAsync(fieldEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(fieldEntity.Id, fieldEntity.GetEvents(fieldEntity.AtSequence)));

                result = success
                    ? Result<FieldRecord>.Success(_mapper.Map<FieldRecord>(fieldEntity))
                    : Result<FieldRecord>.Error(FailedToUpdateMessage(command));
            }
            else
            {
                result = Result<FieldRecord>.Error($"StoreVisit does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToUpdateMessage(command));
            result = Result<FieldRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToUpdateMessage(UpdateFieldCommand command) =>
        $"Failed to create field\nCommand: '{JsonConvert.SerializeObject(command)}'";


    private async Task<List<FieldEntity>> GetExistingFieldByLabelAndId(Guid id, string fieldLabel)
    {
        const string idKey = "@id";
        const string fieldLabelKey = "@fieldLabel";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        conditions.Add($"c.id != {idKey}");
        parameters.Add(idKey, id.ToString());

        conditions.Add($"LOWER(c.label) = {fieldLabelKey}");
        parameters.Add(fieldLabelKey, fieldLabel.ToLower());

        if (!conditions.Any())
            return new List<FieldEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedFields = await _checkpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedFields;
    }
}