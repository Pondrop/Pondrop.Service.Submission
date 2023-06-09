﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionTemplateStoreViewCommandHandler : IRequestHandler<UpdateSubmissionTemplateStoreViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<SubmissionTemplateViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateSubmissionTemplateStoreViewCommandHandler> _logger;

    public UpdateSubmissionTemplateStoreViewCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<SubmissionTemplateViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateSubmissionTemplateStoreViewCommandHandler> logger) : base()
    {
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateSubmissionTemplateStoreViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.SubmissionTemplateId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedSubmissionsTask = GetAffectedSubmissionsAsync(command.SubmissionTemplateId);

            await Task.WhenAll(affectedSubmissionsTask);

            var tasks = affectedSubmissionsTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var submissionTemplateView = _mapper.Map<SubmissionTemplateViewRecord>(i);
                    var result = await _containerRepository.UpsertAsync(submissionTemplateView);
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

    private static string FailedToMessage(UpdateSubmissionTemplateStoreViewCommand command) =>
        $"Failed to update submissionTemplate view '{JsonConvert.SerializeObject(command)}'";
}