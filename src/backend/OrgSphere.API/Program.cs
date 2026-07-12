using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrgSphere.API.GraphQL;
using OrgSphere.API.Middleware;
using OrgSphere.Application.DependencyInjection;
using OrgSphere.Application.Services;
using OrgSphere.Domain;
using OrgSphere.Domain.Configuration;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Infrastructure.Auth;
using OrgSphere.Infrastructure.DependencyInjection;
using OrgSphere.Infrastructure.Events;
using OrgSphere.Infrastructure.Services;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/orgsphere-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>()!;
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisSettings.ConnectionString));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IEventBus, InMemoryEventBus>();
builder.Services.AddScoped<IRefreshTokenStore, RedisRefreshTokenStore>();
builder.Services.AddScoped<OrgSphere.Application.Services.IAuthService, AuthService>();
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddScoped<IOrganizationGraphService, OrganizationGraphService>();
builder.Services.AddScoped<IAuthorizationService, OrgSphere.Application.Services.AuthorizationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<OrganizationGraphQuery>()
    .AddMutationType<OrganizationGraphMutation>()
    .AddType<OrgSphere.Application.DTOs.CompanyDto>()
    .AddType<OrgSphere.Application.DTOs.RegionDto>()
    .AddType<OrgSphere.Application.DTOs.OfficeDto>()
    .AddType<OrgSphere.Application.DTOs.DepartmentDto>()
    .AddType<OrgSphere.Application.DTOs.TeamDto>()
    .AddType<OrgSphere.Application.DTOs.OrgEmployeeDto>()
    .AddType<OrgSphere.Application.DTOs.GraphEdgeDto>()
    .AddType<OrgSphere.Domain.Enums.EdgeType>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5001",
                "https://localhost:7001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    await app.Services.RunMigrationsAsync();
    await app.Services.RunSeedDataAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWebApp");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL("/graphql");

app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

app.Run();
