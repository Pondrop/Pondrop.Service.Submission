using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildSubmissionWithStoreViewCommandHandler : IRequestHandler<RebuildSubmissionWithStoreViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<StoreVisitEntity> _storeVisitCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _containerRepository;
    private readonly IContainerRepository<StoreViewRecord> _storeContainerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildSubmissionWithStoreViewCommandHandler> _logger;

    public RebuildSubmissionWithStoreViewCommandHandler(
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<StoreViewRecord> storeContainerRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildSubmissionWithStoreViewCommandHandler> logger) : base()
    {
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _storeVisitCheckpointRepository = storeVisitCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _storeContainerRepository = storeContainerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildSubmissionWithStoreViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var submissionTemplateTask = _submissionTemplateCheckpointRepository.GetAllAsync();
            var submissionsTask = _submissionCheckpointRepository.GetAllAsync();

            await Task.WhenAll(submissionTemplateTask, submissionsTask);

            var submissionTemplateLookup = submissionTemplateTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<SubmissionTemplateRecord>(i));

            var tasks = submissionsTask.Result.Select(async i =>
            {
                var success = false;

                var storeVisit = await _storeVisitCheckpointRepository.GetByIdAsync(i.StoreVisitId);

                Guid storeId = Guid.Empty;
                DateTime submittedUtc = DateTime.MinValue;

                if (storeVisit != null)
                {
                    storeId = storeVisit.StoreId;
                    submittedUtc = i.CreatedUtc;
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
                        StoreName = store?.Name,
                        RetailerName = store?.Retailer?.Name,
                        StoreId = store?.Id ?? Guid.Empty,
                        CampaignId = i?.CampaignId ?? null,
                        TaskType = submissionTemplate.Title,
                        UserId = storeVisit?.UserId,
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
            _logger.LogError(ex, ex.Message);
            result = Result<int>.Error(ex);
        }

        return result;
    }
}