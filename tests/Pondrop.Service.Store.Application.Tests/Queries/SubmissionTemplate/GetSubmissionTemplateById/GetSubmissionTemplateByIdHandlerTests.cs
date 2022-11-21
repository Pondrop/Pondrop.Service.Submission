using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Submission.Application.Queries;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetSubmissionTemplateById;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Tests.Faker;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Queries.SubmissionTemplate.GetSubmissionTemplateById;

public class GetSubmissionTemplateByIdQueryHandlerTests
{
    private readonly Mock<ICheckpointRepository<SubmissionTemplateEntity>> _checkpointRepositoryMock;
    private readonly Mock<IValidator<GetSubmissionTemplateByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetSubmissionTemplateByIdQueryHandler>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;

    public GetSubmissionTemplateByIdQueryHandlerTests()
    {
        _checkpointRepositoryMock = new Mock<ICheckpointRepository<SubmissionTemplateEntity>>();
        _validatorMock = new Mock<IValidator<GetSubmissionTemplateByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetSubmissionTemplateByIdQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async void GetSubmissionByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTemplateEntity?>(new SubmissionTemplateEntity()));
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _checkpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }

    [Fact]
    public async void GetSubmissionByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTemplateEntity?>(new SubmissionTemplateEntity()));
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
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTemplateEntity?>(null));
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
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }

    [Fact]
    public async void GetSubmissionByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
        var item = SubmissionTemplateFaker.GetSubmissionTemplateRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
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
        _checkpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }

    private GetSubmissionTemplateByIdQueryHandler GetQueryHandler() =>
        new GetSubmissionTemplateByIdQueryHandler(
            _checkpointRepositoryMock.Object,
            _validatorMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
}