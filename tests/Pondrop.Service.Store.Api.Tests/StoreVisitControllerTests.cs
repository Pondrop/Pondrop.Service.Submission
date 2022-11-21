using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Models;
using Moq;
using Pondrop.Service.StoreVisit.Api.Controllers;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using Pondrop.Service.Submission.Api.Tests.Faker;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateStoreVisit;
using Pondrop.Service.Submission.Application.Queries.Submission.GetAllStoreVisits;
using Pondrop.Service.Submission.Application.Queries.Submission.GetStoreVisitById;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;
using Pondrop.Service.Interfaces;

namespace Pondrop.Service.Submission.Api.Tests
{
    public class StoreVisitControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ITokenProvider> _jwtProviderMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<StoreVisitController>> _loggerMock;

        public StoreVisitControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _jwtProviderMock = new Mock<ITokenProvider>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<StoreVisitController>>();

            _jwtProviderMock.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(new ClaimsPrincipal());
        }

        [Fact]
        public async void GetAllStoreVisits_ShouldReturnOkResult()
        {
            // arrange
            var items = StoreVisitFaker.GetStoreVisitRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllStoreVisitsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<StoreVisitRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllStoreVisits();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllStoreVisitsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetAllStoreVisits_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<StoreVisitRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllStoreVisitsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllStoreVisits();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllStoreVisitsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetStoreVisitById_ShouldReturnOkResult()
        {
            // arrange
            var item = StoreVisitFaker.GetStoreVisitRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetStoreVisitByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreVisitRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.GetStoreVisitById(item.Id);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetStoreVisitByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetStoreVisitById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreVisitRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetStoreVisitByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetStoreVisitById(Guid.NewGuid());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetStoreVisitByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void CreateStoreVisitCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = StoreVisitFaker.GetCreateStoreVisitCommand();
            var item = StoreVisitFaker.GetStoreVisitRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreVisitRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.CreateStoreVisit(cmd);

            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }

        [Fact]
        public async void CreateStoreVisitCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreVisitRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateStoreVisitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.CreateStoreVisit(new CreateStoreVisitCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateStoreVisitCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateStoreVisitCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = StoreVisitFaker.GetStoreVisitRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreVisitRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.UpdateCheckpoint(cmd);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void UpdateCheckpoint_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreVisitRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateStoreVisitCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.UpdateCheckpoint(new UpdateStoreVisitCheckpointByIdCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateStoreVisitCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void RebuildCheckpoint_ShouldReturnAcceptedResult()
        {
            // arrange
            var controller = GetController();

            // act
            var response = controller.RebuildCheckpoint();

            // assert
            Assert.IsType<AcceptedResult>(response);
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildStoreVisitCheckpointCommand>()), Times.Once());
        }

        private StoreVisitController GetController() =>
            new StoreVisitController(
                _mediatorMock.Object,
                _jwtProviderMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
