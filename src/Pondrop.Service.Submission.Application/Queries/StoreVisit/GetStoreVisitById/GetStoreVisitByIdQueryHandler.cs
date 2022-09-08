using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitById;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Queries;

public class GetStoreVisitByIdQueryHandler : IRequestHandler<GetStoreVisitByIdQuery, Result<StoreVisitViewRecord?>>
{
    private readonly IContainerRepository<StoreVisitViewRecord> _viewRepository;
    private readonly IValidator<GetStoreVisitByIdQuery> _validator;
    private readonly ILogger<GetStoreVisitByIdQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetStoreVisitByIdQueryHandler(
        IContainerRepository<StoreVisitViewRecord> viewRepository,
        IValidator<GetStoreVisitByIdQuery> validator,
        IUserService userService,
        ILogger<GetStoreVisitByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<StoreVisitViewRecord?>> Handle(GetStoreVisitByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get store visit by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreVisitViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<StoreVisitViewRecord?>);

        try
        {
            var records = await _viewRepository.QueryAsync($"SELECT * FROM c WHERE c.userId = '{_userService.CurrentUserId()}' AND c.id = '{query.Id}' OFFSET 0 LIMIT 1");
            result = records is not null
                ? Result<StoreVisitViewRecord?>.Success(records.FirstOrDefault())
                : Result<StoreVisitViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<StoreVisitViewRecord?>.Error(ex);
        }

        return result;
    }
}