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

namespace Pondrop.Service.Submission.Application.Tests.Commands.Submission.CreateSubmission;

public class GetSubmissionByIdHandlerTests
{
    private readonly Mock<IContainerRepository<SubmissionViewRecord>> _storeContainerRepositoryMock;
    private readonly Mock<IValidator<GetSubmissionByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetSubmissionByIdQueryHandler>> _loggerMock;
    
    public GetSubmissionByIdHandlerTests()
    {
        _storeContainerRepositoryMock = new Mock<IContainerRepository<SubmissionViewRecord>>();
        _validatorMock = new Mock<IValidator<GetSubmissionByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetSubmissionByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetSubmissionByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionViewRecord?>(new SubmissionViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetSubmissionByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionViewRecord?>(new SubmissionViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
    }
    
    [Fact]
    public async void GetSubmissionByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionViewRecord?>(null));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetSubmissionByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
        var item = SubmissionFaker.GetSubmissionTemplateRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    private GetSubmissionByIdQueryHandler GetQueryHandler() =>
        new GetSubmissionByIdQueryHandler(
            _storeContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}