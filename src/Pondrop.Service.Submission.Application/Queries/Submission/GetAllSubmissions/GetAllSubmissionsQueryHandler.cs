﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissions;

public class GetAllSubmissionsQueryHandler : IRequestHandler<GetAllSubmissionsQuery, Result<List<SubmissionViewRecord>>>
{
    private readonly IContainerRepository<SubmissionViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllSubmissionsQuery> _validator;
    private readonly ILogger<GetAllSubmissionsQueryHandler> _logger;

    public GetAllSubmissionsQueryHandler(
        IContainerRepository<SubmissionViewRecord> storeRepository,
        IMapper mapper,
        IValidator<GetAllSubmissionsQuery> validator,
        ILogger<GetAllSubmissionsQueryHandler> logger)
    {
        _containerRepository = storeRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<SubmissionViewRecord>>> Handle(GetAllSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all submissionTemplate templates failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SubmissionViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SubmissionViewRecord>>);

        try
        {
            var records = await _containerRepository.GetAllAsync();
            result = Result<List<SubmissionViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SubmissionViewRecord>>.Error(ex);
        }

        return result;
    }
}