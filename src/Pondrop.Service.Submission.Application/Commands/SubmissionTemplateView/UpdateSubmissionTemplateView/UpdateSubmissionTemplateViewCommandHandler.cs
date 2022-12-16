using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionTemplateViewCommandHandler : IRequestHandler<UpdateSubmissionTemplateViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly ICheckpointRepository<FieldEntity> _fieldCheckpointRepository;
    private readonly IContainerRepository<SubmissionTemplateViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateSubmissionTemplateViewCommandHandler> _logger;

    public UpdateSubmissionTemplateViewCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        ICheckpointRepository<FieldEntity> fieldCheckpointRepository,
        IContainerRepository<SubmissionTemplateViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateSubmissionTemplateViewCommandHandler> logger) : base()
    {
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _fieldCheckpointRepository = fieldCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateSubmissionTemplateViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.SubmissionTemplateId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var submissionTemplateTask = _submissionTemplateCheckpointRepository.GetByIdAsync(command.SubmissionTemplateId.Value);

            await Task.WhenAll(submissionTemplateTask);


            var success = false;

            try
            {
                if (submissionTemplateTask.Result != null)
                {
                    var submissionTemplate = submissionTemplateTask.Result;
                    var submissionTemplateView = new SubmissionTemplateViewRecord(submissionTemplate.Id,
                                                                                  submissionTemplate.Title,
                                                                                  submissionTemplate.Description,
                                                                                  submissionTemplate.IconCodePoint,
                                                                                  submissionTemplate.IconFontFamily,
                                                                                      submissionTemplate.Type,
                                                                                      submissionTemplate.Status,
                                                                                      submissionTemplate.IsForManualSubmissions,
                                                                                      submissionTemplate.Focus,
                                                                                  new List<StepViewRecord>(),
                                                                                  submissionTemplate.CreatedBy,
                                                                                  submissionTemplate.UpdatedBy,
                                                                                  submissionTemplate.CreatedUtc,
                                                                                  submissionTemplate.UpdatedUtc);

                    foreach (var step in submissionTemplate.Steps)
                    {
                        var fields = new List<FieldRecord>();
                        foreach (var field in step.FieldDefinitions)
                        {
                            var retrievedField = await _fieldCheckpointRepository.GetByIdAsync(field.Id);
                            if (retrievedField != null)
                            {
                                var fieldRecord = _mapper.Map<FieldRecord>(retrievedField) with
                                {
                                    Label = field.Label ?? retrievedField.Label,
                                    MaxValue = field.MaxValue ?? retrievedField.MaxValue,
                                    Mandatory = field.Mandatory ?? retrievedField.Mandatory,
                                };

                                fields.Add(fieldRecord);
                            }
                        }

                        submissionTemplateView.Steps.Add(new StepViewRecord(step.Id,
                                                                        step.Title,
                                                                        step.Instructions,
                                                                        step.InstructionsStep,
                                                                        step.InstructionsContinueButton,
                                                                        step.InstructionsSkipButton,
                                                                        step.InstructionsIconCodePoint,
                                                                        step.InstructionsIconFontFamily,
                                                                        step.IsSummary,
                                                                        fields,
                                                                        step.CreatedBy,
                                                                        step.UpdatedBy,
                                                                        step.CreatedUtc,
                                                                        step.UpdatedUtc,
                                                                        step.DeletedUtc));
                    }

                    var view = await _containerRepository.UpsertAsync(submissionTemplateView);
                    success = result != null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update submissionTemplate view for '{command.SubmissionTemplateId}'");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }

    private async Task<List<SubmissionTemplateEntity>> GetAffectedSubmissionsAsync(Guid? submissionTemplateId)
    {
        const string submissionTemplateIdKey = "@submissionTemplateId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (submissionTemplateId.HasValue)
        {
            conditions.Add($"c.id = {submissionTemplateIdKey}");
            parameters.Add(submissionTemplateIdKey, submissionTemplateId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<SubmissionTemplateEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedSubmissions = await _submissionTemplateCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSubmissions;
    }

    private static string FailedToMessage(UpdateSubmissionTemplateViewCommand command) =>
        $"Failed to update submissionTemplate view '{JsonConvert.SerializeObject(command)}'";
}