using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class RebuildSubmissionTemplateStoreViewCommandHandler : IRequestHandler<RebuildSubmissionTemplateStoreViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<SubmissionTemplateViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildSubmissionTemplateStoreViewCommandHandler> _logger;

    public RebuildSubmissionTemplateStoreViewCommandHandler(
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<SubmissionTemplateViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildSubmissionTemplateStoreViewCommandHandler> logger) : base()
    {
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildSubmissionTemplateStoreViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var submissionTemplatesTask = _submissionTemplateCheckpointRepository.GetAllAsync();

            await Task.WhenAll(submissionTemplatesTask);

            var tasks = submissionTemplatesTask.Result.Select(async i =>
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
            _logger.LogError(ex, $"Failed to rebuild submissionTemplate view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}