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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();