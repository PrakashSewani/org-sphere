using Microsoft.Extensions.Logging;
using OrgSphere.Infrastructure.Seed.Seeds;

namespace OrgSphere.Infrastructure.Seed;

public class SeedDataRunner(
    TenantSeed tenantSeed,
    UserSeed userSeed,
    GraphSeed graphSeed,
    ILogger<SeedDataRunner> logger) : ISeedDataRunner
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Running seed data...");

        var tenantId = await tenantSeed.SeedAsync(cancellationToken);
        if (tenantId is null)
        {
            logger.LogInformation("Seed data already exists, skipping");
            return;
        }

        await userSeed.SeedAsync(tenantId!, cancellationToken);
        await graphSeed.SeedAsync(tenantId!, cancellationToken);

        logger.LogInformation("Seed data completed successfully");
    }
}
