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
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.RemoveStepFromSubmission;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Queries;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetAllSubmissionTemplates;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetSubmissionTemplateById;
using Pondrop.Service.Submission.Domain.Events.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Pondrop.Service.Submission.Api.Tests
{
    public class SubmissionTemplateControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ITokenProvider> _jwtProviderMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<SubmissionTemplateController>> _loggerMock;

        public SubmissionTemplateControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _jwtProviderMock = new Mock<ITokenProvider>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<SubmissionTemplateController>>();

            _jwtProviderMock.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(new ClaimsPrincipal());
        }

        [Fact]
        public async void GetAllSubmissions_ShouldReturnOkResult()
        {
            // arrange
            var items = SubmissionFaker.GetSubmissionTemplateRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllSubmissionTemplatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<SubmissionTemplateRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllSubmissionTemplates();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllSubmissionTemplatesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetAllSubmissions_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<SubmissionTemplateRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllSubmissionTemplatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllSubmissionTemplates();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllSubmissionTemplatesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetSubmissionById_ShouldReturnOkResult()
        {
            // arrange
            var item = SubmissionFaker.GetSubmissionTemplateRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetSubmissionTemplateByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionTemplateRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.GetSubmissionTemplateById(item.Id);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetSubmissionTemplateByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void GetSubmissionById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<SubmissionTemplateRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetSubmissionTemplateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetSubmissionTemplateById(Guid.NewGuid());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetSubmissionTemplateByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void CreateSubmissionCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = SubmissionFaker.GetCreateSubmissionTemplateCommand();
            var item = SubmissionFaker.GetSubmissionTemplateRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionTemplateRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.CreateSubmissionTemplate(cmd);

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
            var failedResult = Result<SubmissionTemplateRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateSubmissionTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.CreateSubmissionTemplate(new CreateSubmissionTemplateCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateSubmissionTemplateCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void AddStepToSubmissionCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = SubmissionFaker.GetAddStepCommand();
            var item = SubmissionFaker.GetSubmissionTemplateRecords(1).Single() with { Id = cmd.SubmissionId };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionTemplateRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.AddStepToSubmissionTemplate(cmd);

            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }

        [Fact]
        public async void AddAddressToSubmissionCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<SubmissionTemplateRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<AddStepToSubmissionTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.AddStepToSubmissionTemplate(new AddStepToSubmissionTemplateCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<AddStepToSubmissionTemplateCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void RemoveAddressToSubmissionCommand_ShouldReturnOkResult()
        {
            // arrange
            var item = SubmissionFaker.GetSubmissionTemplateRecords(1).Single();
            var cmd = new RemoveStepFromSubmissionTemplateCommand()
            {
                Id = item.Steps.First().Id,
                SubmissionTemplateId = item.Id
            };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionTemplateRecord>.Success(item));
            var controller = GetController();

            // act
            var response = await controller.RemoveStepFromTemplate(cmd);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }


        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateSubmissionTemplateCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = SubmissionFaker.GetSubmissionTemplateRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<SubmissionTemplateRecord>.Success(item));
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
            var failedResult = Result<SubmissionTemplateRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateSubmissionTemplateCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.UpdateCheckpoint(new UpdateSubmissionTemplateCheckpointByIdCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateSubmissionTemplateCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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

        private SubmissionTemplateController GetController() =>
            new SubmissionTemplateController(
                _mediatorMock.Object,
                _jwtProviderMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
