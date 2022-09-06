using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetSubmissionTemplateById;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionTemplateByIdQueryHandler : IRequestHandler<GetSubmissionTemplateByIdQuery, Result<SubmissionTemplateViewRecord?>>
{
    private readonly IContainerRepository<SubmissionTemplateViewRecord> _viewRepository;
    private readonly IValidator<GetSubmissionTemplateByIdQuery> _validator;
    private readonly ILogger<GetSubmissionTemplateByIdQueryHandler> _logger;

    public GetSubmissionTemplateByIdQueryHandler(
        IContainerRepository<SubmissionTemplateViewRecord> viewRepository,
        IValidator<GetSubmissionTemplateByIdQuery> validator,
        ILogger<GetSubmissionTemplateByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<SubmissionTemplateViewRecord?>> Handle(GetSubmissionTemplateByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get submissionTemplate template by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionTemplateViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<SubmissionTemplateViewRecord?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);
            result = record is not null
                ? Result<SubmissionTemplateViewRecord?>.Success(record)
                : Result<SubmissionTemplateViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<SubmissionTemplateViewRecord?>.Error(ex);
        }

        return result;
    }
}