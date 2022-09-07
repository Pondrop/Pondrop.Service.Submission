using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Commands;

public class UpdateStoreVisitViewCommandHandler : IRequestHandler<UpdateStoreVisitViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<StoreVisitEntity> _submissionCheckpointRepository;
    private readonly IContainerRepository<StoreVisitViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateStoreVisitViewCommandHandler> _logger;

    public UpdateStoreVisitViewCommandHandler(
        ICheckpointRepository<StoreVisitEntity> submissionCheckpointRepository,
        IContainerRepository<StoreVisitViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateStoreVisitViewCommandHandler> logger) : base()
    {
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateStoreVisitViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.StoreVisitId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedStoreVisitsTask = GetAffectedStoreVisitsAsync(command.StoreVisitId);

            await Task.WhenAll(affectedStoreVisitsTask);

            var tasks = affectedStoreVisitsTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var submissionView = _mapper.Map<StoreVisitViewRecord>(i);
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

    private async Task<List<StoreVisitEntity>> GetAffectedStoreVisitsAsync(Guid? submissionTemplateId)
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
            return new List<StoreVisitEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedStoreVisits = await _submissionCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStoreVisits;
    }

    private static string FailedToMessage(UpdateStoreVisitViewCommand command) =>
        $"Failed to update submissionTemplate view '{JsonConvert.SerializeObject(command)}'";
}