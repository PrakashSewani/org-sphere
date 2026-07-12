namespace OrgSphere.Infrastructure.Migrations;

public interface IMigrationRunner
{
    Task RunAllAsync(CancellationToken cancellationToken = default);
}
