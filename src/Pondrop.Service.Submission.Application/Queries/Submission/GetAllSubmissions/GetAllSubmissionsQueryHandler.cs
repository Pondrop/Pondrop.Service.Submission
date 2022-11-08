using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.User;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using System.Net.Http.Headers;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissions;

public class GetAllSubmissionsQueryHandler : IRequestHandler<GetAllSubmissionsQuery, Result<List<SubmissionViewRecord>>>
{
    private readonly IContainerRepository<SubmissionViewRecord> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllSubmissionsQuery> _validator;
    private readonly ILogger<GetAllSubmissionsQueryHandler> _logger;
    private readonly IUserService _userService;


    public GetAllSubmissionsQueryHandler(
        IContainerRepository<SubmissionViewRecord> checkpointRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetAllSubmissionsQuery> validator,
        ILogger<GetAllSubmissionsQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _userService = userService;
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
            var query = $"SELECT * FROM c";
            query += _userService.CurrentUserType() == UserType.Shopper
                ? $" WHERE c.createdBy = '{_userService.CurrentUserId()}'" : string.Empty;

            if (request.Offset != -1 && request.Limit != -1)
            {
                query += $" OFFSET {request.Offset} LIMIT {request.Limit}";
            }


            var records = await _checkpointRepository.QueryAsync(query);

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