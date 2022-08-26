using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionViewCommandHandler : IRequestHandler<UpdateSubmissionViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionCheckpointRepository;
    private readonly IContainerRepository<SubmissionViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateSubmissionViewCommandHandler> _logger;

    public UpdateSubmissionViewCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionCheckpointRepository,
        IContainerRepository<SubmissionViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateSubmissionViewCommandHandler> logger) : base()
    {
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateSubmissionViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.SubmissionId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedSubmissionsTask = GetAffectedSubmissionsAsync(command.SubmissionId);

            await Task.WhenAll(affectedSubmissionsTask);

            var tasks = affectedSubmissionsTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var submissionView = _mapper.Map<SubmissionViewRecord>(i);
                    var result = await _containerRepository.UpsertAsync(submissionView);
                    success = result != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update submission view for '{i.Id}'");
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

    private async Task<List<SubmissionTemplateEntity>> GetAffectedSubmissionsAsync(Guid? submissionId)
    {
        const string submissionIdKey = "@submissionId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (submissionId.HasValue)
        {
            conditions.Add($"c.id = {submissionIdKey}");
            parameters.Add(submissionIdKey, submissionId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<SubmissionTemplateEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedSubmissions = await _submissionCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSubmissions;
    }

    private static string FailedToMessage(UpdateSubmissionViewCommand command) =>
        $"Failed to update submission view '{JsonConvert.SerializeObject(command)}'";
}