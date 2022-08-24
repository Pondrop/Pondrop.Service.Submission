﻿using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Events.Submission;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Commands;

public class AddStepTemplateToSubmissionCommandHandler : DirtyCommandHandler<SubmissionTemplateEntity, AddStepTemplateToSubmissionCommand, Result<SubmissionTemplateRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<AddStepTemplateToSubmissionCommand> _validator;
    private readonly ILogger<AddStepTemplateToSubmissionCommandHandler> _logger;

    public AddStepTemplateToSubmissionCommandHandler(
        IOptions<SubmissionUpdateConfiguration> submissionUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<AddStepTemplateToSubmissionCommand> validator,
        ILogger<AddStepTemplateToSubmissionCommandHandler> logger) : base(eventRepository, submissionUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SubmissionTemplateRecord>> Handle(AddStepTemplateToSubmissionCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create step template failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionTemplateRecord>.Error(errorMessage);
        }

        var result = default(Result<SubmissionTemplateRecord>);

        try
        {
            var submissionEntity = await _submissionCheckpointRepository.GetByIdAsync(command.SubmissionId);
            submissionEntity ??= await GetFromStreamAsync(command.SubmissionId);

            if (submissionEntity is not null)
            {
                var evtPayload = new AddSubmissionStepTemplate(
                    Guid.NewGuid(),
                    submissionEntity.Id,
                    command.Title,
                    command.Type);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(submissionEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _submissionCheckpointRepository.FastForwardAsync(submissionEntity);
                    success = await UpdateStreamAsync(submissionEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(submissionEntity.Id, submissionEntity.GetEvents(submissionEntity.AtSequence)));

                result = success
                    ? Result<SubmissionTemplateRecord>.Success(_mapper.Map<SubmissionTemplateRecord>(submissionEntity))
                    : Result<SubmissionTemplateRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<SubmissionTemplateRecord>.Error("Submission does not exist");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SubmissionTemplateRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(AddStepTemplateToSubmissionCommand command) =>
        $"Failed to create submission step template\nCommand: '{JsonConvert.SerializeObject(command)}'";
}