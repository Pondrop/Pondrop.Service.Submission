using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionTemplateByIdQueryHandler : IRequestHandler<GetSubmissionTemplateByIdQuery, Result<SubmissionViewRecord?>>
{
    private readonly IContainerRepository<SubmissionViewRecord> _viewRepository;
    private readonly IValidator<GetSubmissionTemplateByIdQuery> _validator;
    private readonly ILogger<GetSubmissionTemplateByIdQueryHandler> _logger;

    public GetSubmissionTemplateByIdQueryHandler(
        IContainerRepository<SubmissionViewRecord> viewRepository,
        IValidator<GetSubmissionTemplateByIdQuery> validator,
        ILogger<GetSubmissionTemplateByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<SubmissionViewRecord?>> Handle(GetSubmissionTemplateByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get submission template by id failed {validation}";
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