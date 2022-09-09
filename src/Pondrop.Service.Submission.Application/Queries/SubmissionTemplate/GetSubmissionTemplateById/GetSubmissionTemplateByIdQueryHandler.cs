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

public class GetSubmissionTemplateByIdQueryHandler : IRequestHandler<GetSubmissionTemplateByIdQuery, Result<SubmissionTemplateRecord?>>
{
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _checkpointRepository;
    private readonly IValidator<GetSubmissionTemplateByIdQuery> _validator;
    private readonly ILogger<GetSubmissionTemplateByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetSubmissionTemplateByIdQueryHandler(
        ICheckpointRepository<SubmissionTemplateEntity> checkpointRepository,
        IValidator<GetSubmissionTemplateByIdQuery> validator,
        IMapper mapper,
        ILogger<GetSubmissionTemplateByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SubmissionTemplateRecord?>> Handle(GetSubmissionTemplateByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get submissionTemplate template by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionTemplateRecord?>.Error(errorMessage);
        }

        var result = default(Result<SubmissionTemplateRecord?>);

        try
        {
            var entity = await _checkpointRepository.GetByIdAsync(query.Id);
            result = entity is not null
                ? Result<SubmissionTemplateRecord?>.Success(_mapper.Map<SubmissionTemplateRecord>(entity))
                : Result<SubmissionTemplateRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<SubmissionTemplateRecord?>.Error(ex);
        }

        return result;
    }
}