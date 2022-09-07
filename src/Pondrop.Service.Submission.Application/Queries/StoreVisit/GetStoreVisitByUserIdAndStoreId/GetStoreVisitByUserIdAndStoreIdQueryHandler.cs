using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitByUserIdAndStoreId;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.StoreVisit.Application.Queries;

public class GetStoreVisitByUserIdAndStoreIdQueryHandler : IRequestHandler<GetStoreVisitByUserIdAndStoreIdQuery, Result<StoreVisitViewRecord?>>
{
    private readonly IContainerRepository<StoreVisitViewRecord> _viewRepository;
    private readonly IValidator<GetStoreVisitByUserIdAndStoreIdQuery> _validator;
    private readonly ILogger<GetStoreVisitByUserIdAndStoreIdQueryHandler> _logger;

    public GetStoreVisitByUserIdAndStoreIdQueryHandler(
        IContainerRepository<StoreVisitViewRecord> viewRepository,
        IValidator<GetStoreVisitByUserIdAndStoreIdQuery> validator,
        ILogger<GetStoreVisitByUserIdAndStoreIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<StoreVisitViewRecord?>> Handle(GetStoreVisitByUserIdAndStoreIdQuery query, CancellationToken cancellationToken)
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
            var record = await _viewRepository.GetByIdAsync(query.StoreId);
            result = record is not null
                ? Result<StoreVisitViewRecord?>.Success(record)
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