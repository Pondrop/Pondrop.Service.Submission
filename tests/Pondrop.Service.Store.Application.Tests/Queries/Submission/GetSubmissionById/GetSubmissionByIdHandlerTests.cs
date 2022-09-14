using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Submission.Api.Tests.Faker;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Queries;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Queries.Submission.GetSubmissionById;

public class GetSubmissionByIdQueryHandlerTests
{
    private readonly Mock<ICheckpointRepository<SubmissionEntity>> _checkpointRepositoryMock;
    private readonly Mock<IValidator<GetSubmissionByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetSubmissionByIdQueryHandler>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserService> _userServiceMock;

    public GetSubmissionByIdQueryHandlerTests()
    {
        _checkpointRepositoryMock = new Mock<ICheckpointRepository<SubmissionEntity>>();
        _validatorMock = new Mock<IValidator<GetSubmissionByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetSubmissionByIdQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _userServiceMock = new Mock<IUserService>();

        _userServiceMock
            .Setup(x => x.CurrentUserId())
            .Returns("test/user");
    }

    [Fact]
    public async void GetSubmissionByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.FromResult<List<SubmissionEntity?>>(new List<SubmissionEntity?>()));
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());

        _checkpointRepositoryMock.Verify(
            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Once());
    }

    [Fact]
    public async void GetSubmissionByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionEntity?>(new SubmissionEntity()));
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _checkpointRepositoryMock.Verify(
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
        _checkpointRepositoryMock
            .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.FromResult<List<SubmissionEntity?>>(null)!);
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());

        _checkpointRepositoryMock.Verify(
            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Once());
    }

    [Fact]
    public async void GetSubmissionByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
        var item = SubmissionFaker.GetSubmissionRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Throws(new Exception());
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());

        _checkpointRepositoryMock.Verify(
            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Once());
    }

    private GetSubmissionByIdQueryHandler GetQueryHandler() =>
        new GetSubmissionByIdQueryHandler(
            _checkpointRepositoryMock.Object,
            _validatorMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
}