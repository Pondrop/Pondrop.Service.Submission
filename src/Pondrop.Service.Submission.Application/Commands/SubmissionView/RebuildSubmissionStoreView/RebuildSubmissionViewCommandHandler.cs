using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildSubmissionViewCommandHandler : IRequestHandler<RebuildSubmissionViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<StoreVisitEntity> _storeVisitCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<SubmissionViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildSubmissionViewCommandHandler> _logger;

    public RebuildSubmissionViewCommandHandler(
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<SubmissionViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildSubmissionViewCommandHandler> logger) : base()
    {
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _storeVisitCheckpointRepository = storeVisitCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildSubmissionViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var storeVisitTask = _storeVisitCheckpointRepository.GetAllAsync();
            var submissionTemplateTask = _submissionTemplateCheckpointRepository.GetAllAsync();
            var submissionsTask = _submissionCheckpointRepository.GetAllAsync();

            await Task.WhenAll(storeVisitTask, submissionTemplateTask, submissionsTask);

            var storeVisitLookup = storeVisitTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<StoreVisitViewRecord>(i));
            var submissionTemplateLookup = submissionTemplateTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<SubmissionTemplateRecord>(i));


            await Task.WhenAll(storeVisitTask, submissionTemplateTask, submissionsTask);

            var tasks = submissionsTask.Result.Select(async i =>
        {
            var success = false;

            try
            {
                var submissionView = _mapper.Map<SubmissionViewRecord>(i) with
                {
                    //SubmissionTemplate = submissionTemplateLookup[i.SubmissionTemplateId]
                };

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
            _logger.LogError(ex, $"Failed to rebuild submission view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}