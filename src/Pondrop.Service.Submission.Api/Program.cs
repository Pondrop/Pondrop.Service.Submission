using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pondrop.Service.Submission.Api.Configurations.Extensions;
using Pondrop.Service.Submission.Api.Middleware;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using Pondrop.Service.Submission.Infrastructure.CosmosDb;
using Pondrop.Service.Submission.Infrastructure.Dapr;
using Pondrop.Service.Submission.Infrastructure.ServiceBus;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
{
    ContractResolver = new DefaultContractResolver()
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    },
    DateTimeZoneHandling = DateTimeZoneHandling.Utc
};

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true);

services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Add services to the container.
services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

services.AddLogging(config =>
{
    config.AddDebug();
    config.AddConsole();
});

services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    var Key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JWT:Issuer"],
        ValidAudience = configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Key)
    };
});

services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddAutoMapper(
    typeof(Result<>),
    typeof(EventEntity),
    typeof(EventRepository));
services.AddMediatR(
    typeof(Result<>));
services.AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining(typeof(Result<>));
    });

services.Configure<CosmosConfiguration>(configuration.GetSection(CosmosConfiguration.Key));
services.Configure<ServiceBusConfiguration>(configuration.GetSection(ServiceBusConfiguration.Key));
services.Configure<SubmissionUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(SubmissionUpdateConfiguration.Key));

services.AddHostedService<ServiceBusHostedService>();
services.AddSingleton<IServiceBusListenerService, ServiceBusListenerService>();

services.AddHostedService<RebuildMaterializeViewHostedService>();
services.AddSingleton<IRebuildCheckpointQueueService, RebuildCheckpointQueueService>();

services.AddSingleton<IAddressService, AddressService>();
services.AddSingleton<IUserService, UserService>();
services.AddSingleton<IEventRepository, EventRepository>();
services.AddSingleton<ICheckpointRepository<SubmissionTemplateEntity>, CheckpointRepository<SubmissionTemplateEntity>>();
services.AddSingleton<IContainerRepository<SubmissionViewRecord>, ContainerRepository<SubmissionViewRecord>>();
services.AddSingleton<IDaprService, DaprService>();
services.AddSingleton<IServiceBusService, ServiceBusService>();
services.AddSingleton<ITokenProvider, JWTTokenProvider>();

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();
app.UseSwaggerDocumentation(provider);

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
