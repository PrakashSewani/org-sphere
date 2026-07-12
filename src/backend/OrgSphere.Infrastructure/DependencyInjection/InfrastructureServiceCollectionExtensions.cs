using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Infrastructure.Migrations;
using OrgSphere.Infrastructure.Migrations.Migrations;
using OrgSphere.Infrastructure.Persistence;
using OrgSphere.Infrastructure.Seed;
using OrgSphere.Infrastructure.Seed.Seeds;

namespace OrgSphere.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var neo4jUri = configuration["Neo4j:Uri"] ?? "bolt://localhost:7687";
        var neo4jUser = configuration["Neo4j:User"] ?? "neo4j";
        var neo4jPassword = configuration["Neo4j:Password"] ?? "password";

        services.AddSingleton<IDriver>(sp =>
            GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPassword)));

        services.AddScoped<INeo4jContext, Neo4jContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IMigration, CreateConstraints>();
        services.AddSingleton<IMigration, CreateIndexes>();
        services.AddSingleton<IMigration, CreateTenantCompositeIndexes>();
        services.AddSingleton<IMigration, CreateFullTextIndexes>();
        services.AddScoped<IMigrationRunner, MigrationRunner>();

        services.AddScoped<TenantSeed>();
        services.AddScoped<UserSeed>();
        services.AddScoped<GraphSeed>();
        services.AddScoped<ISeedDataRunner, SeedDataRunner>();

        return services;
    }

    public static async Task RunMigrationsAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        await runner.RunAllAsync(ct);
    }

    public static async Task RunSeedDataAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<ISeedDataRunner>();
        await runner.RunAsync(ct);
    }
}
