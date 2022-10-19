using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using Pondrop.Service.Submission.Application.Queries.Campaign.GetAllCampaigns;
using Pondrop.Service.Submission.Application.Queries.Campaign.GetCampaignById;
using Pondrop.Service.Submission.Application.Commands.Submission.CreateCampaign;
using Pondrop.Service.Campaign.Application.Commands;

namespace Pondrop.Service.Submission.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CampaignController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly ITokenProvider _jwtTokenProvider;
    private readonly ILogger<CampaignController> _logger;

    public CampaignController(
        IMediator mediator,
        ITokenProvider jWTTokenProvider,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        ILogger<CampaignController> logger)
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
    public async Task<IActionResult> GetAllCampaigns()
    {
        var result = await _mediator.Send(new GetAllCampaignsQuery());
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
    public async Task<IActionResult> GetCampaignById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetCampaignByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateCampaignCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPut]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCampaign([FromBody] UpdateCampaignCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateCampaignCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateCampaignCheckpointByIdCommand command)
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
        _rebuildCheckpointQueueService.Queue(new RebuildCampaignCheckpointCommand());
        return new AcceptedResult();
    }

}
