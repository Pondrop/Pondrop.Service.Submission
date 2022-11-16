using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionWithStoreViewCommandHandler : IRequestHandler<UpdateSubmissionWithStoreViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<StoreVisitEntity> _storeVisitCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateSubmissionWithStoreViewCommandHandler> _logger;
    private readonly IContainerRepository<StoreViewRecord> _storeContainerRepository;

    public UpdateSubmissionWithStoreViewCommandHandler(
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<StoreViewRecord> storeContainerRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateSubmissionWithStoreViewCommandHandler> logger) : base()
    {
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _storeVisitCheckpointRepository = storeVisitCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _storeContainerRepository = storeContainerRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateSubmissionWithStoreViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.StoreId.HasValue && !command.SubmissionId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var submissionTemplateTask = _submissionTemplateCheckpointRepository.GetAllAsync();
            var affectedStoreVisitTask = GetAffectedStoreVisitsAsync(command.StoreId);

            await Task.WhenAll(submissionTemplateTask, affectedStoreVisitTask);

            var submissionTemplateLookup = submissionTemplateTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<SubmissionTemplateRecord>(i));
            var affectedStoreVisitLookup = affectedStoreVisitTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<StoreVisitRecord>(i));

            var submissions = await GetAffectedSubmissionsAsync(affectedStoreVisitLookup.Select(s => s.Key).ToList(), command.SubmissionId);

            var tasks = submissions.Select(async i =>
            {
                var success = false;

                Guid storeId = Guid.Empty;
                Guid userId = Guid.Empty;
                DateTime submittedUtc = DateTime.MinValue;

                if (affectedStoreVisitLookup.Count > 0)
                {
                    var storeVisit = affectedStoreVisitLookup[i.StoreVisitId];
                    submittedUtc = i.CreatedUtc;
                        userId = storeVisit.UserId;
                    storeId = storeVisit.StoreId;
                }
                else
                {
                    var storeVisit = await _storeVisitCheckpointRepository.GetByIdAsync(i.StoreVisitId);
                    if (storeVisit != null)
                    {
                        storeId = storeVisit.StoreId;
                        userId = storeVisit.UserId;
                        submittedUtc = i.CreatedUtc;
                    }
                }

                var store = await _storeContainerRepository.GetByIdAsync(storeId);

                var submissionTemplate = submissionTemplateLookup[i.SubmissionTemplateId];

                if (submissionTemplate == null)
                    return false;

                List<string> stepsWithImages = new List<string>();

                try
                {
                    foreach (var step in i.Steps)
                    {
                        foreach (var field in step.Fields)
                        {
                            if (field.Values.Any(v => !string.IsNullOrEmpty(v.PhotoUrl)))
                            {
                                stepsWithImages.Add(submissionTemplate?.Steps?.FirstOrDefault(s => s.Id == step.TemplateStepId)?.Title);
                                continue;
                            }
                        }
                    }

                    var submissionView = _mapper.Map<SubmissionWithStoreViewRecord>(i) with
                    {
                        StoreName = command.Name ?? store?.Name,
                        RetailerName = command.RetailerName ?? store?.Retailer?.Name,
                        StoreId = store?.Id ?? Guid.Empty,
                        CampaignId = i?.CampaignId ?? null,
                        TaskType = submissionTemplate.Title,
                        UserId = userId,
                        SubmittedUtc = submittedUtc,
                        Images = string.Join(',', stepsWithImages)
                    };

                    var result = await _containerRepository.UpsertAsync(submissionView);
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

    private async Task<List<StoreVisitEntity>> GetAffectedStoreVisitsAsync(Guid? storeId)
    {
        const string storeIdKey = "@storeId";
        const string storeVisitIdKey = "@storeVisitId";


        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (storeId.HasValue)
        {
            conditions.Add($"c.storeId = {storeIdKey}");
            parameters.Add(storeIdKey, storeId.Value.ToString());
        }


        if (!conditions.Any())
            return new List<StoreVisitEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedStoreVisits = await _storeVisitCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStoreVisits;
    }

    private async Task<List<SubmissionEntity>> GetAffectedSubmissionsAsync(List<Guid> storeVisitIds, Guid? submissionId)
    {
        const string submissionIdKey = "@submissionId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (storeVisitIds is { Count: > 0 })
        {
            conditions.Add($"c.storeVisitId in ({string.Join(",", storeVisitIds.Select(s => $"'{s}'").ToList())})");
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

    private static string FailedToMessage(UpdateSubmissionWithStoreViewCommand command) =>
            $"Failed to update submissionTemplate view '{JsonConvert.SerializeObject(command)}'";
}