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

public class GetAllStoreVisitsQueryHandler : IRequestHandler<GetAllStoreVisitsQuery, Result<List<StoreVisitViewRecord>>>
{
    private readonly IContainerRepository<StoreVisitViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllStoreVisitsQuery> _validator;
    private readonly ILogger<GetAllStoreVisitsQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetAllStoreVisitsQueryHandler(
        IContainerRepository<StoreVisitViewRecord> storeRepository,
        IUserService userService,
        IMapper mapper,
        IValidator<GetAllStoreVisitsQuery> validator,
        ILogger<GetAllStoreVisitsQueryHandler> logger)
    {
        _containerRepository = storeRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<StoreVisitViewRecord>>> Handle(GetAllStoreVisitsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all store visits failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<StoreVisitViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<StoreVisitViewRecord>>);

        try
        {
            var records = await _containerRepository.QueryAsync($"SELECT * FROM c WHERE c.userId = '{_userService.CurrentUserId()}'");
            result = Result<List<StoreVisitViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<StoreVisitViewRecord>>.Error(ex);
        }

        return result;
    }
}