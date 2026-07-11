using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Infrastructure.Persistence;

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

        return services;
    }
}
