using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitByStoreId;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Queries;

public class GetStoreVisitByStoreIdQueryHandler : IRequestHandler<GetStoreVisitByStoreIdQuery, Result<List<StoreVisitViewRecord>?>>
{
    private readonly IContainerRepository<List<StoreVisitViewRecord>> _viewRepository;
    private readonly IValidator<GetStoreVisitByStoreIdQuery> _validator;
    private readonly ILogger<GetStoreVisitByStoreIdQueryHandler> _logger;
    private readonly IUserService _userService;
    private readonly IContainerRepository<StoreVisitViewRecord> _containerRepository;
    private readonly IMapper _mapper;

    public GetStoreVisitByStoreIdQueryHandler(
        IContainerRepository<StoreVisitViewRecord> storeRepository,
        IUserService userService,
        IMapper mapper,
        IValidator<GetStoreVisitByStoreIdQuery> validator,
        ILogger<GetStoreVisitByStoreIdQueryHandler> logger)
    {
        _containerRepository = storeRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<StoreVisitViewRecord>>> Handle(GetStoreVisitByStoreIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get store visit by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<StoreVisitViewRecord>?>.Error(errorMessage);
        }

        var result = default(Result<List<StoreVisitViewRecord>?>);

        try
        {
            var records = await _containerRepository.QueryAsync($"SELECT * FROM c WHERE c.userId = '{_userService.CurrentUserId()}' AND c.storeId = '{query.StoreId}' OFFSET 0 LIMIT 1");
            result = Result<List<StoreVisitViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<StoreVisitViewRecord>?>.Error(ex);
        }

        return result;
    }
}