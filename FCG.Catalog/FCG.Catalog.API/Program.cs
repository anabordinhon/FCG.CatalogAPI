using FCG.Catalog.API.Common;
using FCG.Catalog.Application.GamePurchases.Ports;
using FCG.Catalog.Application.GamePurchases.UseCases.Commands.AddGamePurchase;
using FCG.Catalog.Application.GamePurchases.UseCases.Queries;
using FCG.Catalog.Application.Games.UseCases.Commands.AddGame;
using FCG.Catalog.Application.Games.UseCases.Queries.GetGameById;
using FCG.Catalog.Application.Games.UseCases.Queries.GetGamesPaged;
using FCG.Catalog.Application.Promotions.UseCases.Commands.AddPromotion;
using FCG.Catalog.Application.Promotions.UseCases.Queries.GetPromotionById;
using FCG.Catalog.Application.Promotions.UseCases.Queries.GetPromotionsPaged;
using FCG.Catalog.Domain.Common.Ports;
using FCG.Catalog.Domain.Games.Ports;
using FCG.Catalog.Domain.Promotions.Ports;
using FCG.Catalog.Infraestructure.Adapters.GamePurchases;
using FCG.Catalog.Infraestructure.Adapters.Games.Repositories;
using FCG.Catalog.Infraestructure.Adapters.Promotions.Repositories;
using FCG.Catalog.Infraestructure.Adapters.Promotions.Services;
using FCG.Catalog.Infraestructure.Persistence;
using FCG.Catalog.Infraestructure.Persistence.Interceptors;
using FCG.Catalog.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.ParseStateValues = true;
});
var awsLoggerConfig = new AWS.Logger.AWSLoggerConfig
{
    Region = builder.Configuration["AWS:Region"] ?? "us-east-1",
    LogGroup = builder.Configuration["AWS.Logging:LogGroup"] ?? "/fcg/catalog/api"
};
builder.Logging.AddAWSProvider(awsLoggerConfig);

const string serviceName = "FCG.Catalog";
const string serviceVersion = "1.0.0";

var collectorEndpoint = builder.Configuration["OpenTelemetry:CollectorEndpoint"]
    ?? "http://host.docker.internal:4317";

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddSource("MassTransit")
        .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .SetSampler(new AlwaysOnSampler())
        .AddConsoleExporter()
    )
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(collectorEndpoint);
            opts.Protocol = OtlpExportProtocol.Grpc;
        })
    )
    .WithLogging(logging => logging
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddConsoleExporter()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(collectorEndpoint);
            opts.Protocol = OtlpExportProtocol.Grpc;
        })
    );

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var userContext = serviceProvider.GetService<IUserContext>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString);
    options.AddInterceptors(new AuditInterceptor(userContext));
}, ServiceLifetime.Scoped);

builder.Services.AddHttpContextAccessor();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API está funcionando"));

builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IAddOrUpdatePromotionCommandHandler, AddOrUpdatePromotionCommandHandler>();
builder.Services.AddScoped<IPromotionCommandRepository, PromotionCommandRepository>();
builder.Services.AddScoped<IPromotionQueryRepository, PromotionQueryRepository>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IGetPromotionByIdQueryHandler, GetPromotionByIdQueryHandler>();
builder.Services.AddScoped<IGetPromotionsPagedQueryHandler, GetPromotionsPagedQueryHandler>();

builder.Services.AddScoped<IAddOrUpdateGameCommandHandler, AddOrUpdateGameCommandHandler>();
builder.Services.AddScoped<IGameCommandRepository, GameCommandRepository>();
builder.Services.AddScoped<IGameQueryRepository, GameQueryRepository>();
builder.Services.AddScoped<IGetGameByIdQueryHandler, GetGameByIdQueryHandler>();
builder.Services.AddScoped<IGetGamesPagedQueryHandler, GetGamesPagedQueryHandler>();

builder.Services.AddScoped<IAddGamePurchasesCommandHandler, AddGamePurchasesCommandHandler>();
builder.Services.AddScoped<IGamePurchaseCommandRepository, GamePurchasesCommandRepository>();
builder.Services.AddScoped<IGamePurchaseQueryRepository, GamePurchaseQueryRepository>();
builder.Services.AddScoped<IGetByUserGamePurchasesQueryHandler, GetByUserGamePurchasesQueryHandler>();

builder.Services.AddMessaging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fiap Cloud Games API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
  new OpenApiSecurityScheme
     {
    Reference = new OpenApiReference
  {
      Type = ReferenceType.SecurityScheme,
        Id = "Bearer"
      }
  },
   new string[] {}
      }
    });
});

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey não configurada (verifique appsettings / User Secrets)");

Console.WriteLine($"=== JWT CONFIG CATALOG ===");
Console.WriteLine($"JWT Key Length: {jwtSecretKey.Length}");
Console.WriteLine($"JWT Key (primeiros 10 chars): {jwtSecretKey.Substring(0, Math.Min(10, jwtSecretKey.Length))}...");
Console.WriteLine($"=========================");

var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();