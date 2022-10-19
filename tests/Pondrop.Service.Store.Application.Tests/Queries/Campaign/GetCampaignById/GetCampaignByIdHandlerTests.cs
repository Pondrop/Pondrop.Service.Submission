using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Submission.Api.Tests.Faker;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Queries;
using Pondrop.Service.Submission.Application.Queries.Campaign.GetCampaignById;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Submission.Application.Tests.Queries.Campaign.GetCampaignById;

public class GetCampaignByIdQueryHandlerTests
{
    private readonly Mock<ICheckpointRepository<CampaignEntity>> _checkpointRepositoryMock;
    private readonly Mock<IValidator<GetCampaignByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetCampaignByIdQueryHandler>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;

    public GetCampaignByIdQueryHandlerTests()
    {
        _checkpointRepositoryMock = new Mock<ICheckpointRepository<CampaignEntity>>();
        _validatorMock = new Mock<IValidator<GetCampaignByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetCampaignByIdQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
    }
    [Fact]
    public async void GetSubmissionByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetCampaignByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult<CampaignEntity?>(new CampaignEntity())!);
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());

        _checkpointRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once());
    }

    [Fact]
    public async void GetSubmissionByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetCampaignByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<CampaignEntity?>(new CampaignEntity()));
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
        var query = new GetCampaignByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult<CampaignEntity?>(null));
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
            x => x.GetByIdAsync(It.IsAny<Guid>()),
        Times.Once());
    }

    [Fact]
    public async void GetSubmissionByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetCampaignByIdQuery() { Id = Guid.NewGuid() };
        var item = CampaignFaker.GetCampaignRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
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
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once());
    }

    private GetCampaignByIdQueryHandler GetQueryHandler() =>
        new GetCampaignByIdQueryHandler(
            _checkpointRepositoryMock.Object,
            _validatorMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
}