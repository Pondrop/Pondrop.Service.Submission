using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetAllSubmissionTemplates;

public class GetAllSubmissionTemplatesQueryHandler : IRequestHandler<GetAllSubmissionTemplatesQuery, Result<List<SubmissionTemplateViewRecord>>>
{
    private readonly IContainerRepository<SubmissionTemplateViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllSubmissionTemplatesQuery> _validator;
    private readonly ILogger<GetAllSubmissionTemplatesQueryHandler> _logger;

    public GetAllSubmissionTemplatesQueryHandler(
        IContainerRepository<SubmissionTemplateViewRecord> storeRepository,
        IMapper mapper,
        IValidator<GetAllSubmissionTemplatesQuery> validator,
        ILogger<GetAllSubmissionTemplatesQueryHandler> logger)
    {
        _containerRepository = storeRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<SubmissionTemplateViewRecord>>> Handle(GetAllSubmissionTemplatesQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all submissionTemplate templates failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SubmissionTemplateViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SubmissionTemplateViewRecord>>);

        try
        {
            var records = await _containerRepository.GetAllAsync();
            result = Result<List<SubmissionTemplateViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SubmissionTemplateViewRecord>>.Error(ex);
        }

        return result;
    }
}