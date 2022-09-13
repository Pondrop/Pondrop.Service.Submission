using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.User;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissions;

public class GetAllSubmissionsQueryHandler : IRequestHandler<GetAllSubmissionsQuery, Result<List<SubmissionRecord>>>
{
    private readonly ICheckpointRepository<SubmissionEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllSubmissionsQuery> _validator;
    private readonly ILogger<GetAllSubmissionsQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetAllSubmissionsQueryHandler(
        ICheckpointRepository<SubmissionEntity> checkpointRepository,
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

    public async Task<Result<List<SubmissionRecord>>> Handle(GetAllSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all submissionTemplate templates failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SubmissionRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SubmissionRecord>>);

        try
        {
            var query = $"SELECT * FROM c";
            query += _userService.CurrentUserType() == UserType.Shopper
                ? $" WHERE c.createdBy = '{_userService.CurrentUserId()}'" : string.Empty;

            var entities = await _checkpointRepository.QueryAsync(query);
            result = Result<List<SubmissionRecord>>.Success(_mapper.Map<List<SubmissionRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SubmissionRecord>>.Error(ex);
        }

        return result;
    }
}