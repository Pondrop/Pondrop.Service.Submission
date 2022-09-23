//using AutoMapper;
//using FluentValidation;
//using FluentValidation.Results;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Pondrop.Service.Store.Domain.Models;
//using Pondrop.Service.Submission.Api.Tests.Faker;
//using Pondrop.Service.Submission.Application.Interfaces;
//using Pondrop.Service.Submission.Application.Interfaces.Services;
//using Pondrop.Service.Submission.Application.Queries;
//using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;
//using Pondrop.Service.Submission.Domain.Models;
//using Pondrop.Service.Submission.Domain.Models.StoreVisit;
//using Pondrop.Service.Submission.Domain.Models.Submission;
//using Pondrop.Service.Submission.Tests.Faker;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;

//namespace Pondrop.Service.Submission.Application.Tests.Queries.Submission.GetSubmissionById;

//public class GetSubmissionByIdQueryHandlerTests
//{
//    private readonly Mock<ICheckpointRepository<SubmissionEntity>> _submissionCheckpointRepositoryMock;
//    private readonly Mock<ICheckpointRepository<StoreVisitEntity>> _storeVisitCheckpointRepositoryMock;
//    private readonly Mock<ICheckpointRepository<SubmissionTemplateEntity>> _submissionTemplateCheckpointRepositoryMock;
//    private readonly Mock<IContainerRepository<SubmissionWithStoreViewRecord>> _containerRepositoryMock;
//    private readonly Mock<IMapper> _mapperMock;
//    private readonly Mock<IUserService> _userServiceMock;
//    private readonly Mock<ILogger<GetSubmissionByIdQueryHandler>> _loggerMock;
//    private readonly Mock<IContainerRepository<StoreViewRecord>> _storeContainerRepositoryMock;
//    private readonly Mock<IValidator<GetSubmissionByIdQuery>> _validatorMock;

//    public GetSubmissionByIdQueryHandlerTests()

//    {

//        _submissionCheckpointRepositoryMock = new Mock<ICheckpointRepository<SubmissionEntity>>();
//        _storeVisitCheckpointRepositoryMock = new Mock<ICheckpointRepository<StoreVisitEntity>>();
//        _submissionTemplateCheckpointRepositoryMock = new Mock<ICheckpointRepository<SubmissionTemplateEntity>>();
//        _containerRepositoryMock = new Mock<IContainerRepository<SubmissionWithStoreViewRecord>>();
//        _storeContainerRepositoryMock = new Mock<IContainerRepository<StoreViewRecord>>();
//        _mapperMock = new Mock<IMapper>();
//        _userServiceMock = new Mock<IUserService>();
//        _loggerMock = new Mock<ILogger<GetSubmissionByIdQueryHandler>>();
//        _validatorMock = new Mock<IValidator<GetSubmissionByIdQuery>>();

//        _userServiceMock
//            .Setup(x => x.CurrentUserId())
//            .Returns("test/user");
//    }

//    [Fact]
//    public async void GetSubmissionByIdQuery_ShouldSucceed()
//    {
//        // arrange
//        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
//        _validatorMock
//            .Setup(x => x.Validate(query))
//            .Returns(new ValidationResult());
//        _submissionCheckpointRepositoryMock
//            .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
//            .Returns(Task.FromResult<List<SubmissionEntity?>>(SubmissionFaker.GetSubmissionEntities()));
//        _storeVisitCheckpointRepositoryMock
//          .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
//          .Returns(Task.FromResult(StoreVisitFaker.GetStoreVisitEntity()));
//        _submissionTemplateCheckpointRepositoryMock
//            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
//            .Returns(Task.FromResult(SubmissionTemplateFaker.GetSubmissionTemplateEntity()));
//        _storeContainerRepositoryMock
//            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
//            .Returns(Task.FromResult<StoreViewRecord>(new StoreViewRecord()));
//        var handler = GetQueryHandler();

//        // act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // assert
//        Assert.True(result.IsSuccess);
//        _validatorMock.Verify(
//            x => x.Validate(query),
//            Times.Once());

//        _submissionCheckpointRepositoryMock.Verify(
//            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
//            Times.Once());
//    }

//    [Fact]
//    public async void GetSubmissionByIdQuery_WhenInvalid_ShouldFail()
//    {
//        // arrange
//        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
//        _validatorMock
//            .Setup(x => x.Validate(query))
//            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
//        _submissionCheckpointRepositoryMock
//            .Setup(x => x.GetByIdAsync(query.Id))
//            .Returns(Task.FromResult<SubmissionEntity?>(new SubmissionEntity()));
//        var handler = GetQueryHandler();

//        // act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // assert
//        Assert.False(result.IsSuccess);
//        _validatorMock.Verify(
//            x => x.Validate(query),
//            Times.Once());
//        _submissionCheckpointRepositoryMock.Verify(
//            x => x.GetByIdAsync(query.Id),
//            Times.Never());
//    }

//    [Fact]
//    public async void GetSubmissionByIdQuery_WhenNotFound_ShouldSucceedWithNull()
//    {
//        // arrange
//        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
//        _validatorMock
//            .Setup(x => x.Validate(query))
//            .Returns(new ValidationResult());
//        _submissionCheckpointRepositoryMock
//            .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
//            .Returns(Task.FromResult<List<SubmissionEntity?>>(null)!);
//        var handler = GetQueryHandler();

//        // act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // assert
//        Assert.True(result.IsSuccess);
//        Assert.Null(result.Value);
//        _validatorMock.Verify(
//            x => x.Validate(query),
//            Times.Once());

//        _submissionCheckpointRepositoryMock.Verify(
//            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
//            Times.Once());
//    }

//    [Fact]
//    public async void GetSubmissionByIdQuery_WhenThrows_ShouldFail()
//    {
//        // arrange
//        var query = new GetSubmissionByIdQuery() { Id = Guid.NewGuid() };
//        var item = SubmissionFaker.GetSubmissionRecords(1).Single();
//        _validatorMock
//            .Setup(x => x.Validate(query))
//            .Returns(new ValidationResult());
//        _submissionCheckpointRepositoryMock
//            .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
//            .Throws(new Exception());
//        var handler = GetQueryHandler();

//        // act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // assert
//        Assert.False(result.IsSuccess);
//        _validatorMock.Verify(
//            x => x.Validate(query),
//            Times.Once());

//        _submissionCheckpointRepositoryMock.Verify(
//            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
//            Times.Once());
//    }

//    private GetSubmissionByIdQueryHandler GetQueryHandler() =>
//        new GetSubmissionByIdQueryHandler(
//            _submissionCheckpointRepositoryMock.Object,
//            _storeVisitCheckpointRepositoryMock.Object,
//            _submissionTemplateCheckpointRepositoryMock.Object,
//            _storeContainerRepositoryMock.Object,
//            _containerRepositoryMock.Object,
//            _validatorMock.Object,
//            _mapperMock.Object,
//            _userServiceMock.Object,
//            _loggerMock.Object);
//}