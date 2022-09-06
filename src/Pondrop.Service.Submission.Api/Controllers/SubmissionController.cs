using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateSubmission;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissions;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using System.Security.Claims;

namespace Pondrop.Service.Submission.Api.Controllers;

[Authorize]
[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class SubmissionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly ITokenProvider _jwtTokenProvider;
    private readonly ILogger<SubmissionController> _logger;

    public SubmissionController(
        IMediator mediator,
        ITokenProvider jWTTokenProvider,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        ILogger<SubmissionController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
        _jwtTokenProvider = jWTTokenProvider;
        _rebuildCheckpointQueueService = rebuildCheckpointQueueService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllSubmissions()
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(new GetAllSubmissionsQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubmissionById([FromRoute] Guid id)
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(new GetSubmissionByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubmission([FromBody] CreateSubmissionCommand command)
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateSubmissionCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateSubmissionCheckpointByIdCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("rebuild/checkpoint")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult RebuildCheckpoint()
    {
        _rebuildCheckpointQueueService.Queue(new RebuildSubmissionCheckpointCommand());
        return new AcceptedResult();
    }

}
