﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;

public class GetAllCampaignsQueryHandler : IRequestHandler<GetAllCampaignsQuery, Result<List<CampaignRecord>>>
{
    private readonly ICheckpointRepository<CampaignEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllCampaignsQuery> _validator;
    private readonly ILogger<GetAllCampaignsQueryHandler> _logger;

    public GetAllCampaignsQueryHandler(
        ICheckpointRepository<CampaignEntity> checkpointRepository,
        IMapper mapper,
        IValidator<GetAllCampaignsQuery> validator,
        ILogger<GetAllCampaignsQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<CampaignRecord>>> Handle(GetAllCampaignsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all campaigns failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CampaignRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<CampaignRecord>>);

        try
        {
            var query = $"SELECT * FROM c";

            if (request.Offset != -1 && request.Limit != -1)
            {
                query += $" OFFSET {request.Offset} LIMIT {request.Limit}";
            }
            var entities = await _checkpointRepository.QueryAsync(query);
            result = Result<List<CampaignRecord>>.Success(_mapper.Map<List<CampaignRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CampaignRecord>>.Error(ex);
        }

        return result;
    }
}