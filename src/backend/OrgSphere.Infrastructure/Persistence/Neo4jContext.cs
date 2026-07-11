using Neo4j.Driver;

namespace OrgSphere.Infrastructure.Persistence;

public interface INeo4jContext : IDisposable
{
    IAsyncSession AsyncSession();
}

public class Neo4jContext : INeo4jContext
{
    private readonly IDriver _driver;

    public Neo4jContext(IDriver driver)
    {
        _driver = driver;
    }

    public IAsyncSession AsyncSession() => _driver.AsyncSession();

    public void Dispose()
    {
        _driver?.Dispose();
    }
}
