namespace OrgSphere.Infrastructure.Seed;

public interface ISeedDataRunner
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
