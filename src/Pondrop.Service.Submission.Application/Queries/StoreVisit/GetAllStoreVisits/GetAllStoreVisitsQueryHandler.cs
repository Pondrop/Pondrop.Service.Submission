using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetAllStoreVisits;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Queries.StoreVisit.GetAllStoreVisits;

public class GetAllStoreVisitsQueryHandler : IRequestHandler<GetAllStoreVisitsQuery, Result<List<StoreVisitRecord>>>
{
    private readonly ICheckpointRepository<StoreVisitEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllStoreVisitsQuery> _validator;
    private readonly ILogger<GetAllStoreVisitsQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetAllStoreVisitsQueryHandler(
        ICheckpointRepository<StoreVisitEntity> checkpointRepository,
        IUserService userService,
        IMapper mapper,
        IValidator<GetAllStoreVisitsQuery> validator,
        ILogger<GetAllStoreVisitsQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<StoreVisitRecord>>> Handle(GetAllStoreVisitsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all store visits failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<StoreVisitRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<StoreVisitRecord>>);

        try
        {
            var entities = await _checkpointRepository.QueryAsync($"SELECT * FROM c WHERE c.userId = '{_userService.CurrentUserId()}'");
            result = Result<List<StoreVisitRecord>>.Success(_mapper.Map<List<StoreVisitRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<StoreVisitRecord>>.Error(ex);
        }

        return result;
    }
}