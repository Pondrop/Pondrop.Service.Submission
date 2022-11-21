using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Models.User;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissionsWithStore;

public class GetAllSubmissionsWithStoreQueryHandler : IRequestHandler<GetAllSubmissionsWithStoreQuery, Result<List<SubmissionWithStoreViewRecord>>>
{
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllSubmissionsWithStoreQuery> _validator;
    private readonly ILogger<GetAllSubmissionsWithStoreQueryHandler> _logger;
    private readonly IUserService _userService;


    public GetAllSubmissionsWithStoreQueryHandler(
        IContainerRepository<SubmissionWithStoreViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetAllSubmissionsWithStoreQuery> validator,
        ILogger<GetAllSubmissionsWithStoreQueryHandler> logger)
    {
        _containerRepository = containerRepository;
        _mapper = mapper;
        _validator = validator;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<List<SubmissionWithStoreViewRecord>>> Handle(GetAllSubmissionsWithStoreQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all submissionTemplate templates failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SubmissionWithStoreViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SubmissionWithStoreViewRecord>>);

        try
        {
            var query = $"SELECT * FROM c";
            query += _userService.CurrentUserType() == UserType.Shopper
                ? $" WHERE c.createdBy = '{_userService.CurrentUserId()}'" : string.Empty;

            if (request.Offset != -1 && request.Limit != -1)
            {
                query += $" OFFSET {request.Offset} LIMIT {request.Limit}";
            }

            var records = await _containerRepository.QueryAsync(query);

            result = Result<List<SubmissionWithStoreViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SubmissionWithStoreViewRecord>>.Error(ex);
        }

        return result;
    }
}