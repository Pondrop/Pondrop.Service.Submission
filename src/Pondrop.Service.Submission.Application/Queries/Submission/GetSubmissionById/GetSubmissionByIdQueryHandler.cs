using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;
using Pondrop.Service.Submission.Domain.Enums.User;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, Result<SubmissionRecord?>>
{
    private readonly ICheckpointRepository<SubmissionEntity> _checkpointRepository;
    private readonly IValidator<GetSubmissionByIdQuery> _validator;
    private readonly ILogger<GetSubmissionByIdQueryHandler> _logger;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetSubmissionByIdQueryHandler(
        ICheckpointRepository<SubmissionEntity> checkpointRepository,
        IValidator<GetSubmissionByIdQuery> validator,
        IUserService userService,
        IMapper mapper,
        ILogger<GetSubmissionByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _userService = userService;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SubmissionRecord?>> Handle(GetSubmissionByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get submissionTemplate template by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionRecord?>.Error(errorMessage);
        }

        var result = default(Result<SubmissionRecord?>);

        try
        {
            var queryString = $"SELECT * FROM c WHERE c.id = '{query.Id}'";
            queryString += _userService.CurrentUserType() == UserType.Shopper
                ? $" AND c.createdBy = '{_userService.CurrentUserId()}'" : string.Empty;

            queryString += " OFFSET 0 LIMIT 1";

            var entity = await _checkpointRepository.QueryAsync(queryString);
            result = entity is not null
                ? Result<SubmissionRecord?>.Success(_mapper.Map<SubmissionRecord>(entity.FirstOrDefault()))
                : Result<SubmissionRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<SubmissionRecord?>.Error(ex);
        }

        return result;
    }
}