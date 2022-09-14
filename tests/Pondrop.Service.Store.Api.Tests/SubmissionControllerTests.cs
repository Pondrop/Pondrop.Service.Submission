using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Moq;
using Pondrop.Service.Submission.Api.Controllers;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using Pondrop.Service.Submission.Api.Tests.Faker;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Queries;
using Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissions;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;
using Pondrop.Service.Submission.Domain.Events.Submission;
using Pondrop.Service.Submission.Domain.Models.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Pondrop.Service.Submission.Api.Tests
{
    public class SubmissionControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ITokenProvider> _jwtProviderMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<SubmissionController>> _loggerMock;

        public SubmissionControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _jwtProviderMock = new Mock<ITokenProvider>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<SubmissionController>>();

            _jwtProviderMock.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(new ClaimsPrincipal());
        }

        [Fact]
        public async void GetAllSubmissions_ShouldReturnOkResult()
        {
            // arrange
            var items = SubmissionFaker.GetSubmissionRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllSubmissionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<SubmissionRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllSubmissions();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllSubmissionsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetAllSubmissions_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<SubmissionRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllSubmissionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllSubmissions();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllSubmissionsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetSubmissionById_ShouldReturnOkResult()
        {
            // arrange
            var item = SubmissionFaker.GetSubmissionRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetSubmissionByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.GetSubmissionById(item.Id);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetSubmissionByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetSubmissionById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<SubmissionRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetSubmissionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetSubmissionById(Guid.NewGuid());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetSubmissionByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void CreateSubmissionCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = SubmissionFaker.GetCreateSubmissionCommand();
            var item = SubmissionFaker.GetSubmissionRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.CreateSubmission(cmd);

            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }

        [Fact]
        public async void CreateSubmissionCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<SubmissionRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateSubmissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.CreateSubmission(new CreateSubmissionCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateSubmissionCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateSubmissionCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = SubmissionFaker.GetSubmissionRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionRecord>.Success(item));
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
            var failedResult = Result<SubmissionRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateSubmissionCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.UpdateCheckpoint(new UpdateSubmissionCheckpointByIdCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateSubmissionCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildSubmissionCheckpointCommand>()), Times.Once());
        }

        private SubmissionController GetController() =>
            new SubmissionController(
                _mediatorMock.Object,
                _jwtProviderMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
