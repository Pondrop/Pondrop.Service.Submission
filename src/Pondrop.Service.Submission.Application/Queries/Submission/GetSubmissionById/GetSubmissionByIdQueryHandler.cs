using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, Result<SubmissionViewRecord?>>
{
    private readonly IContainerRepository<SubmissionViewRecord> _viewRepository;
    private readonly IValidator<GetSubmissionByIdQuery> _validator;
    private readonly ILogger<GetSubmissionByIdQueryHandler> _logger;

    public GetSubmissionByIdQueryHandler(
        IContainerRepository<SubmissionViewRecord> viewRepository,
        IValidator<GetSubmissionByIdQuery> validator,
        ILogger<GetSubmissionByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<SubmissionViewRecord?>> Handle(GetSubmissionByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get submissionTemplate template by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<SubmissionViewRecord?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);
            result = record is not null
                ? Result<SubmissionViewRecord?>.Success(record)
                : Result<SubmissionViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<SubmissionViewRecord?>.Error(ex);
        }

        return result;
    }
}