using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Field.GetFieldById;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetFieldByIdQueryHandler : IRequestHandler<GetFieldByIdQuery, Result<FieldRecord?>>
{
    private readonly ICheckpointRepository<FieldEntity> _checkpointRepository;
    private readonly IValidator<GetFieldByIdQuery> _validator;
    private readonly ILogger<GetFieldByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetFieldByIdQueryHandler(
        ICheckpointRepository<FieldEntity> checkpointRepository,
        IValidator<GetFieldByIdQuery> validator,
        IMapper mapper,
        ILogger<GetFieldByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<FieldRecord?>> Handle(GetFieldByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get field by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<FieldRecord?>.Error(errorMessage);
        }

        var result = default(Result<FieldRecord?>);

        try
        {
            var entity = await _checkpointRepository.GetByIdAsync(query.Id);
            result = entity is not null
                ? Result<FieldRecord?>.Success(_mapper.Map<FieldRecord>(entity))
                : Result<FieldRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<FieldRecord?>.Error(ex);
        }

        return result;
    }
}