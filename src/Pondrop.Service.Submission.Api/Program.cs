using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Submission.Api.Configurations.Extensions;
using Pondrop.Service.Submission.Api.Middleware;
using Pondrop.Service.Submission.Api.Models;
using Pondrop.Service.Submission.Api.Services;
using Pondrop.Service.Submission.Api.Services.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.BlobStorage;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using Pondrop.Service.Submission.Infrastructure.BlobStorage;
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
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var AllowedOrigins = "allowedOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowedOrigins,
        policy =>
        {
            policy.WithOrigins("https://admin-portal.ashyocean-bde16918.australiaeast.azurecontainerapps.io",
                    "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
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
services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

var storeDatabaseName = configuration["StoreCosmosConfiguration:DatabaseName"];
var storeConnectionString = configuration["StoreCosmosConfiguration:ConnectionString"];
var storeApplicationName = configuration["StoreCosmosConfiguration:ApplicationName"];

services.Configure<CosmosConfiguration>(configuration.GetSection(CosmosConfiguration.Key));
services.Configure<StoreCosmosConfiguration>(configuration.GetSection(StoreCosmosConfiguration.Key));
services.Configure<BlobStorageConfiguration>(configuration.GetSection(BlobStorageConfiguration.Key));
services.Configure<ServiceBusConfiguration>(configuration.GetSection(ServiceBusConfiguration.Key));
services.Configure<SearchIndexConfiguration>(configuration.GetSection(SearchIndexConfiguration.Key));
services.Configure<SubmissionUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(SubmissionUpdateConfiguration.Key));

services.AddHostedService<ServiceBusHostedService>();
services.AddSingleton<IServiceBusListenerService, ServiceBusListenerService>();

services.AddHostedService<RebuildMaterializeViewHostedService>();
services.AddSingleton<IRebuildCheckpointQueueService, RebuildCheckpointQueueService>();

services.AddSingleton<IAddressService, AddressService>();
services.AddSingleton<IUserService, UserService>();
services.AddSingleton<IEventRepository, EventRepository>();
services.AddSingleton<IBlobStorageService, BlobStorageService>();
services.AddSingleton<ICheckpointRepository<SubmissionEntity>, CheckpointRepository<SubmissionEntity>>();
services.AddSingleton<ICheckpointRepository<SubmissionTemplateEntity>, CheckpointRepository<SubmissionTemplateEntity>>();
services.AddSingleton<IContainerRepository<StoreVisitViewRecord>, ContainerRepository<StoreVisitViewRecord>>();
services.AddSingleton<ICheckpointRepository<StoreVisitEntity>, CheckpointRepository<StoreVisitEntity>>();
services.AddSingleton<ICheckpointRepository<FieldEntity>, CheckpointRepository<FieldEntity>>();
services.AddSingleton<IContainerRepository<SubmissionTemplateViewRecord>, ContainerRepository<SubmissionTemplateViewRecord>>();
services.AddSingleton<IContainerRepository<SubmissionWithStoreViewRecord>, ContainerRepository<SubmissionWithStoreViewRecord>>();
services.AddSingleton<IContainerRepository<SubmissionViewRecord>, ContainerRepository<SubmissionViewRecord>>();
services.AddSingleton<IContainerRepository<StoreViewRecord>, ContainerRepository<StoreViewRecord>>(provider =>
    new ContainerRepository<StoreViewRecord>(
        Options.Create(new CosmosConfiguration()
        {
            ConnectionString = storeConnectionString,
            ApplicationName = storeApplicationName,
            DatabaseName = storeDatabaseName
        }), null));

services.AddSingleton<IDaprService, DaprService>();
services.AddSingleton<IServiceBusService, ServiceBusService>();
services.AddSingleton<ITokenProvider, JWTTokenProvider>();

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();
app.UseSwaggerDocumentation(provider);

app.UseCors(AllowedOrigins);
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
