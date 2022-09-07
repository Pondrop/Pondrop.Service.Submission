using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Commands;

public class RebuildStoreVisitViewCommandHandler : IRequestHandler<RebuildStoreVisitViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<StoreVisitEntity> _submissionTemplateCheckpointRepository;
    private readonly IContainerRepository<StoreVisitViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildStoreVisitViewCommandHandler> _logger;

    public RebuildStoreVisitViewCommandHandler(
        ICheckpointRepository<StoreVisitEntity> submissionTemplateCheckpointRepository,
        IContainerRepository<StoreVisitViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildStoreVisitViewCommandHandler> logger) : base()
    {
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildStoreVisitViewCommand command, CancellationToken cancellationToken)
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
                    var storeVisitView = _mapper.Map<StoreVisitViewRecord>(i);

                    var result = await _containerRepository.UpsertAsync(storeVisitView);
                    success = result != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update store visit view for '{i.Id}'");
                }

                return success;
            }).ToList();

            await Task.WhenAll(tasks);

            result = Result<int>.Success(tasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to rebuild store visit view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}