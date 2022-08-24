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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Commands.Submission.CreateSubmission;

public class CreateSubmissionCommandHandlerTests
{
    private readonly Mock<IOptions<SubmissionUpdateConfiguration>> _SubmissionUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<ICheckpointRepository<RetailerEntity>> _retailerViewRepositoryMock;
    private readonly Mock<ICheckpointRepository<SubmissionTypeEntity>> _storeTypeViewRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateSubmissionCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateSubmissionCommandHandler>> _loggerMock;
    
    public CreateSubmissionCommandHandlerTests()
    {
        _SubmissionUpdateConfigMock = new Mock<IOptions<SubmissionUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _retailerViewRepositoryMock = new Mock<ICheckpointRepository<RetailerEntity>>();
        _storeTypeViewRepositoryMock = new Mock<ICheckpointRepository<SubmissionTypeEntity>>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateSubmissionCommand>>();
        _loggerMock = new Mock<ILogger<CreateSubmissionCommandHandler>>();

        _SubmissionUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new SubmissionUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserName())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateSubmissionCommand_ShouldSucceed()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.SubmissionTypeId))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(new SubmissionTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()))
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
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateSubmissionCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
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
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateSubmissionCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.SubmissionTypeId))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(new SubmissionTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()))
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
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateSubmissionCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.SubmissionTypeId))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(new SubmissionTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()))
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
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateSubmissionCommand_WhenRetailerNotFound_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(null));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.SubmissionTypeId))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(new SubmissionTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _retailerViewRepositoryMock.Verify(
            x =>x.GetByIdAsync(cmd.RetailerId),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void CreateSubmissionCommand_WhenSubmissionTypeNotFound_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.SubmissionTypeId))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(null));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _storeTypeViewRepositoryMock.Verify(
            x =>x.GetByIdAsync(cmd.SubmissionTypeId),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never());
    }
    
    private CreateSubmissionCommandHandler GetCommandHandler() =>
        new CreateSubmissionCommandHandler(
            _SubmissionUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _retailerViewRepositoryMock.Object,
            _storeTypeViewRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}