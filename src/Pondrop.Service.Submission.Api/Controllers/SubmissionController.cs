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
using Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissionsWithStore;
using Pondrop.Service.Submission.Api.Models;
using Microsoft.Extensions.Options;
using AspNetCore.Proxy.Options;
using AspNetCore.Proxy;
using Azure.Search.Documents.Indexes;
using Azure;

namespace Pondrop.Service.Submission.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class SubmissionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly SearchIndexConfiguration _searchIdxConfig;
    private readonly ITokenProvider _jwtTokenProvider;
    private readonly ILogger<SubmissionController> _logger;

    private readonly HttpProxyOptions _searchProxyOptions;
    private readonly SearchIndexerClient _searchIndexerClient;

    public SubmissionController(
        IMediator mediator,
        ITokenProvider jWTTokenProvider,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        IOptions<SearchIndexConfiguration> searchIdxConfig,
        ILogger<SubmissionController> logger)
    {
        _mediator = mediator;

        _searchIdxConfig = searchIdxConfig.Value;
        _serviceBusService = serviceBusService;
        _jwtTokenProvider = jWTTokenProvider;
        _rebuildCheckpointQueueService = rebuildCheckpointQueueService;
        _logger = logger;

        _searchIndexerClient = new SearchIndexerClient(new Uri(_searchIdxConfig.BaseUrl), new AzureKeyCredential(_searchIdxConfig.ManagementKey));

        _searchProxyOptions = HttpProxyOptionsBuilder
            .Instance
            .WithBeforeSend((httpContext, requestMessage) =>
            {
                requestMessage.Headers.Add("api-key", _searchIdxConfig.ApiKey);
                return Task.CompletedTask;
            }).Build();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllSubmissions(int offset = 0, int limit = 10)
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(new GetAllSubmissionsQuery()
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
    [Route("withstore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllSubmissionsWithStore(int offset = 0, int limit = 10)
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(new GetAllSubmissionsWithStoreQuery()
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
                await _searchIndexerClient.RunIndexerAsync(_searchIdxConfig.SubmissionIndexerName);
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

    [HttpGet, HttpPost]
    [Route("search")]
    public Task ProxySearchCatchAll()
    {
        var queryString = this.Request.QueryString.Value?.TrimStart('?') ?? string.Empty;
        var url = Path.Combine(
            _searchIdxConfig.BaseUrl,
            "indexes",
            _searchIdxConfig.SubmissionIndexName,
            $"docs?api-version=2021-04-30-Preview&{queryString}");

        return this.HttpProxyAsync(url, _searchProxyOptions);
    }

    [HttpGet]
    [Route("indexer/run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunIndexer()
    {
        var response = await _searchIndexerClient.RunIndexerAsync(_searchIdxConfig.SubmissionIndexerName);

        if (response.IsError)
            return new BadRequestObjectResult(response.ReasonPhrase);

        return new AcceptedResult();
    }

}
