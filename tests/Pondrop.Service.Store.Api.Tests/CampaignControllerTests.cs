using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Submission.Application.Models;
using Moq;
using Pondrop.Service.Submission.Api.Controllers;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using Pondrop.Service.Submission.Api.Tests.Faker;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateCampaign;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Domain.Models.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;
using Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;
using Pondrop.Service.Submission.Application.Queries.Campaign.GetCampaignById;

namespace Pondrop.Service.Submission.Api.Tests
{
    public class CampaignControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ITokenProvider> _jwtProviderMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<CampaignController>> _loggerMock;

        public CampaignControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _jwtProviderMock = new Mock<ITokenProvider>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<CampaignController>>();

            _jwtProviderMock.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(new ClaimsPrincipal());
        }

        [Fact]
        public async void GetAllCampaigns_ShouldReturnOkResult()
        {
            // arrange
            var items = CampaignFaker.GetCampaignRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllCampaignsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<CampaignRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllCampaigns();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllCampaignsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetAllCampaigns_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<CampaignRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllCampaignsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllCampaigns();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllCampaignsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetCampaignById_ShouldReturnOkResult()
        {
            // arrange
            var item = CampaignFaker.GetCampaignRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetCampaignByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CampaignRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.GetCampaignById(item.Id);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetCampaignByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetCampaignById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CampaignRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetCampaignByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetCampaignById(Guid.NewGuid());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetCampaignByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void CreateCampaignCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = CampaignFaker.GetCreateCampaignCommand();
            var item = CampaignFaker.GetCampaignRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CampaignRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.CreateCampaign(cmd);

            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }

        [Fact]
        public async void CreateCampaignCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CampaignRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateCampaignCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.CreateCampaign(new CreateCampaignCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateCampaignCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateCampaignCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = CampaignFaker.GetCampaignRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CampaignRecord>.Success(item));
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
            var failedResult = Result<CampaignRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateCampaignCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.UpdateCheckpoint(new UpdateCampaignCheckpointByIdCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateCampaignCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildCampaignCheckpointCommand>()), Times.Once());
        }

        private CampaignController GetController() =>
            new CampaignController(
                _mediatorMock.Object,
                _jwtProviderMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
