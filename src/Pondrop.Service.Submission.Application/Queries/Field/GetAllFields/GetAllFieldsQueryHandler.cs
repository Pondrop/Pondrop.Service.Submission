using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.Field.GetAllFields;

public class GetAllFieldsQueryHandler : IRequestHandler<GetAllFieldsQuery, Result<List<FieldRecord>>>
{
    private readonly ICheckpointRepository<FieldEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllFieldsQuery> _validator;
    private readonly ILogger<GetAllFieldsQueryHandler> _logger;

    public GetAllFieldsQueryHandler(
        ICheckpointRepository<FieldEntity> checkpointRepository,
        IMapper mapper,
        IValidator<GetAllFieldsQuery> validator,
        ILogger<GetAllFieldsQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<FieldRecord>>> Handle(GetAllFieldsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all fields failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<FieldRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<FieldRecord>>);

        try
        {
            var entities = await _checkpointRepository.QueryAsync($"SELECT * FROM c OFFSET {request.Offset} LIMIT {request.Limit}");
            result = Result<List<FieldRecord>>.Success(_mapper.Map<List<FieldRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<FieldRecord>>.Error(ex);
        }

        return result;
    }
}