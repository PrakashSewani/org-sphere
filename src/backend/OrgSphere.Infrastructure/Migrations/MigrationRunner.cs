using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Migrations;

public class MigrationRunner(
    INeo4jContext context,
    IEnumerable<IMigration> migrations,
    ILogger<MigrationRunner> logger) : IMigrationRunner
{
    public async Task RunAllAsync(CancellationToken cancellationToken = default)
    {
        var sorted = migrations.OrderBy(m => m.Id).ToList();

        await using var session = context.AsyncSession();

        foreach (var migration in sorted)
        {
            var migrationId = migration.Id;
            var migrationName = migration.Name;

            var alreadyApplied = await IsAppliedAsync(session, migrationId);
            if (alreadyApplied)
            {
                logger.LogDebug("Migration {MigrationId} ({MigrationName}) already applied, skipping", migrationId, migrationName);
                continue;
            }

            logger.LogInformation("Applying migration {MigrationId} ({MigrationName})", migrationId, migrationName);

            var statements = migration.Up();
            foreach (var cypher in statements)
            {
                await session.RunAsync(cypher);
            }

            await RecordAsync(session, migrationId, migrationName);
            logger.LogInformation("Migration {MigrationId} ({MigrationName}) applied successfully", migrationId, migrationName);
        }
    }

    private static async Task<bool> IsAppliedAsync(IAsyncSession session, string migrationId)
    {
        var cursor = await session.RunAsync(
            "MATCH (m:__Migration {Id: $id}) RETURN m.Id AS Id",
            new Dictionary<string, object?> { ["id"] = migrationId });

        var records = await cursor.ToListAsync();
        return records.Count > 0;
    }

    private static async Task RecordAsync(IAsyncSession session, string migrationId, string name)
    {
        await session.RunAsync(
            """
            CREATE (m:__Migration {Id: $id, Name: $name, AppliedAt: datetime()})
            """,
            new Dictionary<string, object?> { ["id"] = migrationId, ["name"] = name });
    }
}
