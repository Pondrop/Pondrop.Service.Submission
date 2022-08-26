﻿using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;

public class CreateSubmissionTemplateCommandHandler : DirtyCommandHandler<SubmissionTemplateEntity, CreateSubmissionTemplateCommand, Result<SubmissionTemplateRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateSubmissionTemplateCommand> _validator;
    private readonly ILogger<CreateSubmissionTemplateCommandHandler> _logger;

    public CreateSubmissionTemplateCommandHandler(
        IOptions<SubmissionUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateSubmissionTemplateCommand> validator,
        ILogger<CreateSubmissionTemplateCommandHandler> logger) : base(eventRepository, storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SubmissionTemplateRecord>> Handle(CreateSubmissionTemplateCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create store failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SubmissionTemplateRecord>.Error(errorMessage);
        }

        var result = default(Result<SubmissionTemplateRecord>);

        try
        {

            var storeEntity = new SubmissionTemplateEntity(
                command.Title,
                command.Description,
                command.IconCodePoint,
                command.IconFontFamily,
                _userService.CurrentUserName());

            foreach (var step in command.Steps)
            {
                storeEntity.Apply(new AddStep(
                    Guid.NewGuid(),
                    storeEntity.Id,
                    step!.Title,
                    step!.Instructions,
                    step!.InstructionsContinueButton,
                    step!.InstructionsSkipButton,
                    step!.InstructionsIconCodePoint,
                    step!.InstructionsIconFontFamily,
                    step!.Fields,
                    _userService.CurrentUserName(),
                    _userService.CurrentUserName()), _userService.CurrentUserName());
            }
            var success = await _eventRepository.AppendEventsAsync(storeEntity.StreamId, 0, storeEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(storeEntity.Id, storeEntity.GetEvents()));

            result = success
                ? Result<SubmissionTemplateRecord>.Success(_mapper.Map<SubmissionTemplateRecord>(storeEntity))
                : Result<SubmissionTemplateRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SubmissionTemplateRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateSubmissionTemplateCommand command) =>
        $"Failed to create store\nCommand: '{JsonConvert.SerializeObject(command)}'";
}