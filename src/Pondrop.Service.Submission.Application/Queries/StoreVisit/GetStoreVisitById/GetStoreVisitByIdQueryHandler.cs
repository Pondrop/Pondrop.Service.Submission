using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitById;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Queries;

public class GetStoreVisitByIdQueryHandler : IRequestHandler<GetStoreVisitByIdQuery, Result<StoreVisitRecord?>>
{
    private readonly ICheckpointRepository<StoreVisitEntity> _checkpointRepository;
    private readonly IValidator<GetStoreVisitByIdQuery> _validator;
    private readonly ILogger<GetStoreVisitByIdQueryHandler> _logger;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetStoreVisitByIdQueryHandler(
        ICheckpointRepository<StoreVisitEntity> checkpointRepository,
        IValidator<GetStoreVisitByIdQuery> validator,
        IUserService userService,
        IMapper mapper,
        ILogger<GetStoreVisitByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _validator = validator;
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StoreVisitRecord?>> Handle(GetStoreVisitByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get store visit by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreVisitRecord?>.Error(errorMessage);
        }

        var result = default(Result<StoreVisitRecord?>);

        try
        {
            var records = await _checkpointRepository.QueryAsync($"SELECT * FROM c WHERE c.userId = '{_userService.CurrentUserId()}' AND c.id = '{query.Id}' OFFSET 0 LIMIT 1");
            result = records is not null
                ? Result<StoreVisitRecord?>.Success(_mapper.Map<StoreVisitRecord>(records.FirstOrDefault()))
                : Result<StoreVisitRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<StoreVisitRecord?>.Error(ex);
        }

        return result;
    }
}