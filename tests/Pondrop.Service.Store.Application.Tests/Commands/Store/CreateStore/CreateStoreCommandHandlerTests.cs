using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
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
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateSubmissionTemplateCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateSubmissionTemplateCommandHandler>> _loggerMock;
    
    public CreateSubmissionCommandHandlerTests()
    {
        _SubmissionUpdateConfigMock = new Mock<IOptions<SubmissionUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateSubmissionTemplateCommand>>();
        _loggerMock = new Mock<ILogger<CreateSubmissionTemplateCommandHandler>>();

        _SubmissionUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new SubmissionUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserId())
            .Returns("test/user");
    }
    
    //[Fact]
    //public async void CreateSubmissionCommand_ShouldSucceed()
    //{
    //    // arrange
    //    var cmd = SubmissionFaker.GetCreateSubmissionTemplateCommand();
    //    var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
    //    _validatorMock
    //        .Setup(x => x.Validate(cmd))
    //        .Returns(new ValidationResult());
    //    _eventRepositoryMock
    //        .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
    //        .Returns(Task.FromResult(true));
    //    _mapperMock
    //        .Setup(x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()))
    //        .Returns(item);
    //    var handler = GetCommandHandler();
        
    //    // act
    //    var result = await handler.Handle(cmd, CancellationToken.None);
        
    //    // assert
    //    Assert.True(result.IsSuccess);
    //    Assert.Equal(item, result.Value);
    //    _validatorMock.Verify(
    //        x => x.Validate(cmd),
    //        Times.Once());
    //    _eventRepositoryMock.Verify(
    //        x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
    //        Times.Once());
    //    _mapperMock.Verify(
    //        x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
    //        Times.Once);
    //}
    
    [Fact]
    public async void CreateSubmissionCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionTemplateCommand();
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
        //_eventRepositoryMock.Verify(
        //    x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
        //    Times.Never());
        _mapperMock.Verify(
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateSubmissionCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionTemplateCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
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
        //_eventRepositoryMock.Verify(
        //    x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
        //    Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateSubmissionCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionTemplateCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
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
        //_eventRepositoryMock.Verify(
        //    x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
        //    Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateSubmissionCommand_WhenRetailerNotFound_ShouldFail()
    {
        // arrange
        var cmd = SubmissionFaker.GetCreateSubmissionTemplateCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
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
        var cmd = SubmissionFaker.GetCreateSubmissionTemplateCommand();
        var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
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
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<SubmissionTemplateRecord>(It.IsAny<SubmissionTemplateEntity>()),
            Times.Never());
    }
    
    private CreateSubmissionTemplateCommandHandler GetCommandHandler() =>
        new CreateSubmissionTemplateCommandHandler(
            _SubmissionUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}