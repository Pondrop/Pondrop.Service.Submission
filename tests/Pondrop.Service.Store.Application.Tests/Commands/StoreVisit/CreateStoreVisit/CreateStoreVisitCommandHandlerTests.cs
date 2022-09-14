using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.StoreVisit.Application.Commands.StoreVisit.CreateStoreVisit;
using Pondrop.Service.Submission.Api.Tests.Faker;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateStoreVisit;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.BlobStorage;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Commands.Submission.CreateSubmission;

public class CreateStoreVisitCommandHandlerTests
{
    private readonly Mock<IOptions<SubmissionUpdateConfiguration>> _SubmissionUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateStoreVisitCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateStoreVisitCommandHandler>> _loggerMock;
    private readonly Mock<IBlobStorageService> _blobStorageMock;

    public CreateStoreVisitCommandHandlerTests()
    {
        _SubmissionUpdateConfigMock = new Mock<IOptions<SubmissionUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateStoreVisitCommand>>();
        _loggerMock = new Mock<ILogger<CreateStoreVisitCommandHandler>>();
        _blobStorageMock = new Mock<IBlobStorageService>();

        _SubmissionUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new SubmissionUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserId())
            .Returns("test/user");
        _blobStorageMock
            .Setup(x => x.UploadImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("test/user");
    }
    
    //[Fact]
    //public async void CreateStoreVisitCommand_ShouldSucceed()
    //{
    //    // arrange
    //    var cmd = StoreVisitFaker.GetCreateStoreVisitCommand();
    //    var item = StoreVisitFaker.GetStoreVisitRecord(cmd);
    //    _validatorMock
    //        .Setup(x => x.Validate(cmd))
    //        .Returns(new ValidationResult());
    //    _eventRepositoryMock
    //        .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
    //        .Returns(Task.FromResult(true));
    //    _mapperMock
    //        .Setup(x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()))
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
    //        x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()),
    //        Times.Once);
    //}
    
    [Fact]
    public async void CreateStoreVisitCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = StoreVisitFaker.GetCreateStoreVisitCommand();
        var item = StoreVisitFaker.GetStoreVisitRecord(cmd);
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
            x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateStoreVisitCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = StoreVisitFaker.GetCreateStoreVisitCommand();
        var item = StoreVisitFaker.GetStoreVisitRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()))
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
            x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateStoreVisitCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = StoreVisitFaker.GetCreateStoreVisitCommand();
        var item = StoreVisitFaker.GetStoreVisitRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()))
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
            x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateStoreVisitCommand_WhenRetailerNotFound_ShouldFail()
    {
        // arrange
        var cmd = StoreVisitFaker.GetCreateStoreVisitCommand();
        var item = StoreVisitFaker.GetStoreVisitRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()))
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
            x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void CreateStoreVisitCommand_WhenSubmissionTypeNotFound_ShouldFail()
    {
        // arrange
        var cmd = StoreVisitFaker.GetCreateStoreVisitCommand();
        var item = StoreVisitFaker.GetStoreVisitRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()))
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
            x => x.Map<StoreVisitRecord>(It.IsAny<SubmissionEntity>()),
            Times.Never());
    }
    
    private CreateStoreVisitCommandHandler GetCommandHandler() =>
        new CreateStoreVisitCommandHandler(
            _SubmissionUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}