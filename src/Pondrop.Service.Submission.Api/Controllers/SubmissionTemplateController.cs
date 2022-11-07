using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.CreateSubmissionTemplate;
using Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.RemoveStepFromSubmission;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetAllSubmissionTemplates;
using Pondrop.Service.Submission.Application.Queries.SubmissionTemplate.GetSubmissionTemplateById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using System.Security.Claims;

namespace Pondrop.Service.Submission.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class SubmissionTemplateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly ITokenProvider _jwtTokenProvider;
    private readonly ILogger<SubmissionTemplateController> _logger;

    public SubmissionTemplateController(
        IMediator mediator,
        ITokenProvider jWTTokenProvider,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        ILogger<SubmissionTemplateController> logger)
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
    public async Task<IActionResult> GetAllSubmissionTemplates(int offset = 0, int limit = 10)
    {
        var result = await _mediator.Send(new GetAllSubmissionTemplatesQuery()
        {
            Offset = offset,
            Limit = limit
        });
        return result.Match<IActionResult>(
            i => new OkObjectResult(new { Items = i, Offset = offset, Limit = limit, count = i!.Count() }
            ),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubmissionTemplateById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetSubmissionTemplateByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubmissionTemplate([FromBody] CreateSubmissionTemplateCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateSubmissionTemplateCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("step/add")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddStepToSubmissionTemplate([FromBody] AddStepToSubmissionTemplateCommand command)
    {

        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateSubmissionTemplateCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    //[HttpPut]
    //[Route("step/update")]
    //[ProducesResponseType(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> UpdateStepToSubmissionTemplate([FromBody] UpdateStepToSubmissionTemplateCommand command)
    //{

    //    var result = await _mediator.Send(command);
    //    return await result.MatchAsync<IActionResult>(
    //        async i =>
    //        {
    //            await _serviceBusService.SendMessageAsync(new UpdateSubmissionTemplateCheckpointByIdCommand() { Id = i!.Id });
    //            return StatusCode(StatusCodes.Status201Created, i);
    //        },
    //        (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    //}


    [HttpDelete]
    [Route("step/remove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveStepFromTemplate([FromBody] RemoveStepFromSubmissionTemplateCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateSubmissionTemplateCheckpointByIdCommand() { Id = i!.Id });
                return new OkObjectResult(i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }


    [AllowAnonymous]
    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateSubmissionTemplateCheckpointByIdCommand command)
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
