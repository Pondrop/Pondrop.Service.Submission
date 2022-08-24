using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Queries;
using Pondrop.Service.Submission.Domain.Events;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Commands.SubmissionType.CreateSubmissionType;

public class GetSubmissionTypeByIdHandlerTests
{
    private readonly Mock<ICheckpointRepository<SubmissionTypeEntity>> _storeTypeCheckpointRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<GetSubmissionTypeByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetSubmissionTypeByIdQueryHandler>> _loggerMock;
    
    public GetSubmissionTypeByIdHandlerTests()
    {
        _storeTypeCheckpointRepositoryMock = new Mock<ICheckpointRepository<SubmissionTypeEntity>>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<GetSubmissionTypeByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetSubmissionTypeByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetSubmissionTypeByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetSubmissionTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = SubmissionTypeFaker.GetSubmissionTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(new SubmissionTypeEntity()));
        _mapperMock
            .Setup(x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Once());
    }
    
    [Fact]
    public async void GetSubmissionTypeByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = SubmissionTypeFaker.GetSubmissionTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(new SubmissionTypeEntity()));
        _mapperMock
            .Setup(x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetSubmissionTypeByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetSubmissionTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = SubmissionTypeFaker.GetSubmissionTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTypeEntity?>(null));
        _mapperMock
            .Setup(x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetSubmissionTypeByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = SubmissionTypeFaker.GetSubmissionTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<SubmissionTypeRecord>(It.IsAny<SubmissionTypeEntity>()),
            Times.Never());
    }
    
    private GetSubmissionTypeByIdQueryHandler GetQueryHandler() =>
        new GetSubmissionTypeByIdQueryHandler(
            _storeTypeCheckpointRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}