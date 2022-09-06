using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Queries;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetSubmissionTemplateById;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using Pondrop.Service.Submission.Tests.Faker;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Queries.SubmissionTemplate.GetSubmissionTemplateById;

public class GetSubmissionTemplateByIdQueryHandlerTests
{
    private readonly Mock<IContainerRepository<SubmissionTemplateViewRecord>> _storeContainerRepositoryMock;
    private readonly Mock<IValidator<GetSubmissionTemplateByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetSubmissionTemplateByIdQueryHandler>> _loggerMock;

    public GetSubmissionTemplateByIdQueryHandlerTests()
    {
        _storeContainerRepositoryMock = new Mock<IContainerRepository<SubmissionTemplateViewRecord>>();
        _validatorMock = new Mock<IValidator<GetSubmissionTemplateByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetSubmissionTemplateByIdQueryHandler>>();
    }

    [Fact]
    public async void GetSubmissionByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTemplateViewRecord?>(new SubmissionTemplateViewRecord()));
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
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTemplateViewRecord?>(new SubmissionTemplateViewRecord()));
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
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<SubmissionTemplateViewRecord?>(null));
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
        var query = new GetSubmissionTemplateByIdQuery() { Id = Guid.NewGuid() };
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

    private GetSubmissionTemplateByIdQueryHandler GetQueryHandler() =>
        new GetSubmissionTemplateByIdQueryHandler(
            _storeContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}