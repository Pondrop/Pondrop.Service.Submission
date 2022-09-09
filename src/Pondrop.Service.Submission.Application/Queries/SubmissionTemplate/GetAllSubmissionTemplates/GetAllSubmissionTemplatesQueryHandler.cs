using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetAllSubmissionTemplates;

public class GetAllSubmissionTemplatesQueryHandler : IRequestHandler<GetAllSubmissionTemplatesQuery, Result<List<SubmissionTemplateRecord>>>
{
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllSubmissionTemplatesQuery> _validator;
    private readonly ILogger<GetAllSubmissionTemplatesQueryHandler> _logger;

    public GetAllSubmissionTemplatesQueryHandler(
        ICheckpointRepository<SubmissionTemplateEntity> checkpointRepository,
        IMapper mapper,
        IValidator<GetAllSubmissionTemplatesQuery> validator,
        ILogger<GetAllSubmissionTemplatesQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<SubmissionTemplateRecord>>> Handle(GetAllSubmissionTemplatesQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all submissionTemplate templates failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SubmissionTemplateRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SubmissionTemplateRecord>>);

        try
        {
            var entities = await _checkpointRepository.GetAllAsync();
            result = Result<List<SubmissionTemplateRecord>>.Success(_mapper.Map<List<SubmissionTemplateRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SubmissionTemplateRecord>>.Error(ex);
        }

        return result;
    }
}