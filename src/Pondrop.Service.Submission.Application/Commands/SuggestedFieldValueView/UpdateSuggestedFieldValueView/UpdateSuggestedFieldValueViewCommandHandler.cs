using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SuggestedFieldValue;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSuggestedFieldValueViewCommandHandler : IRequestHandler<UpdateSuggestedFieldValueViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<StoreVisitEntity> _storeVisitCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<SuggestedFieldValueViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateSuggestedFieldValueViewCommandHandler> _logger;

    public UpdateSuggestedFieldValueViewCommandHandler(
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<SuggestedFieldValueViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateSuggestedFieldValueViewCommandHandler> logger) : base()
    {
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _storeVisitCheckpointRepository = storeVisitCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateSuggestedFieldValueViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.SubmissionId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var storeVisitTask = _storeVisitCheckpointRepository.GetAllAsync();
            var submissionTemplateTask = _submissionTemplateCheckpointRepository.GetAllAsync();
            var affectedSubmissionsTask = GetAffectedSubmissionsAsync(command.StoreVisitId, command.SubmissionTemplateId, command.SubmissionId);

            await Task.WhenAll(storeVisitTask, submissionTemplateTask, affectedSubmissionsTask);

            var storeVisitLookup = storeVisitTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<StoreVisitViewRecord>(i));
            var submissionTemplateLookup = submissionTemplateTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<SubmissionTemplateRecord>(i));


            var tasks = affectedSubmissionsTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var suggestedFieldValueView = _mapper.Map<SuggestedFieldValueViewRecord>(i) with
                    {
                    };
                    var result = await _containerRepository.UpsertAsync(suggestedFieldValueView);
                    success = result != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update submissionTemplate view for '{i.Id}'");
                }

                return success;
            }).ToList();

            await Task.WhenAll(tasks);

            result = Result<int>.Success(tasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }

    private async Task<List<SubmissionEntity>> GetAffectedSubmissionsAsync(Guid? submissionId, Guid? storeVisitId, Guid? submissionTemplateId)
    {
        const string submissionIdKey = "@submissionId";
        const string storeVisitIdKey = "@storeVisitId";
        const string submissionTemplateIdKey = "@submissionTemplateId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (storeVisitId.HasValue)
        {
            conditions.Add($"c.storeVisitId = {storeVisitIdKey}");
            parameters.Add(storeVisitIdKey, storeVisitId.Value.ToString());
        }

        if (submissionTemplateId.HasValue)
        {
            conditions.Add($"c.submissionTemplateId = {submissionTemplateIdKey}");
            parameters.Add(submissionTemplateIdKey, submissionTemplateId.Value.ToString());
        }

        if (submissionId.HasValue)
        {
            conditions.Add($"c.id = {submissionIdKey}");
            parameters.Add(submissionIdKey, submissionId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<SubmissionEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedSubmissions = await _submissionCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSubmissions;
    }

    private static string FailedToMessage(UpdateSuggestedFieldValueViewCommand command) =>
        $"Failed to update submissionTemplate view '{JsonConvert.SerializeObject(command)}'";
}