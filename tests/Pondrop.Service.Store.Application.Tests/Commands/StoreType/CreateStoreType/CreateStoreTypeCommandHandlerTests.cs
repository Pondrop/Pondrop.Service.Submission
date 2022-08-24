using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Commands.SubmissionType.CreateSubmissionType;

public class CreateSubmissionTypeCommandHandlerTests
{
    private readonly Mock<IOptions<SubmissionTypeUpdateConfiguration>> _SubmissionTypeUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateSubmissionTypeCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateSubmissionTypeCommandHandler>> _loggerMock;
    
    public CreateSubmissionTypeCommandHandlerTests()
    {
        _SubmissionTypeUpdateConfigMock = new Mock<IOptions<SubmissionTypeUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateSubmissionTypeCommand>>();
        _loggerMock = new Mock<ILogger<CreateSubmissionTypeCommandHandler>>();

        _SubmissionTypeUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new SubmissionTypeUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserName())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateSubmissionTypeCommand_ShouldSucceed()
    {
        // arrange
        var cmd = SubmissionTypeFaker.GetCreateSubmissionTypeCommand();
        var item = SubmissionTypeFaker.GetSubmissionTypeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateSubmissionTypeCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = SubmissionTypeFaker.GetCreateSubmissionTypeCommand();
        var item = SubmissionTypeFaker.GetSubmissionTypeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateSubmissionTypeCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = SubmissionTypeFaker.GetCreateSubmissionTypeCommand();
        var item = SubmissionTypeFaker.GetSubmissionTypeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateSubmissionTypeCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = SubmissionTypeFaker.GetCreateSubmissionTypeCommand();
        var item = SubmissionTypeFaker.GetSubmissionTypeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Never);
    }
    
    private CreateSubmissionTypeCommandHandler GetCommandHandler() =>
        new CreateSubmissionTypeCommandHandler(
            _SubmissionTypeUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}