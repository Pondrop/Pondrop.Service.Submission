using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Campaign.GetCampaignById;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, Result<CampaignRecord?>>
{
    private readonly ICheckpointRepository<CampaignEntity> _checkpointRepository;
    private readonly IValidator<GetCampaignByIdQuery> _validator;
    private readonly ILogger<GetCampaignByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetCampaignByIdQueryHandler(
        ICheckpointRepository<CampaignEntity> checkpointRepository,
        IValidator<GetCampaignByIdQuery> validator,
        IMapper mapper,
        ILogger<GetCampaignByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CampaignRecord?>> Handle(GetCampaignByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get campaign by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<CampaignRecord?>.Error(errorMessage);
        }

        var result = default(Result<CampaignRecord?>);

        try
        {
            var entity = await _checkpointRepository.GetByIdAsync(query.Id);
            result = entity is not null
                ? Result<CampaignRecord?>.Success(_mapper.Map<CampaignRecord>(entity))
                : Result<CampaignRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<CampaignRecord?>.Error(ex);
        }

        return result;
    }
}