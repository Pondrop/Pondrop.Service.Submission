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

public class RebuildSubmissionTemplateViewCommandHandler : IRequestHandler<RebuildSubmissionTemplateViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly ICheckpointRepository<FieldEntity> _fieldCheckpointRepository;
    private readonly IContainerRepository<SubmissionTemplateViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildSubmissionTemplateViewCommandHandler> _logger;

    public RebuildSubmissionTemplateViewCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        ICheckpointRepository<FieldEntity> fieldCheckpointRepository,
        IContainerRepository<SubmissionTemplateViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildSubmissionTemplateViewCommandHandler> logger) : base()
    {
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _fieldCheckpointRepository = fieldCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildSubmissionTemplateViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var submissionTemplatesTask = _submissionTemplateCheckpointRepository.GetAllAsync();

            await Task.WhenAll(submissionTemplatesTask);

            var success = false;

            try
            {
                var tasks = submissionTemplatesTask.Result.Select(async i =>
                {
                    if (submissionTemplatesTask.Result != null)
                    {
                        var submissionTemplate = i;
                        var submissionTemplateView = new SubmissionTemplateViewRecord(i.Id,
                                                                                      i.Title,
                                                                                      i.Description,
                                                                                      i.IconCodePoint,
                                                                                      i.IconFontFamily,
                                                                                      new List<StepViewRecord>(),
                                                                                      i.CreatedBy,
                                                                                      i.UpdatedBy,
                                                                                      i.CreatedUtc,
                                                                                      i.UpdatedUtc); ;

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
                }).ToList();

                await Task.WhenAll(tasks);

                result = Result<int>.Success(1);
            }
            catch (Exception ex)
            {
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }
    private static string FailedToMessage(RebuildSubmissionTemplateViewCommand command) =>
     $"Failed to update submissionTemplate view '{JsonConvert.SerializeObject(command)}'";
}