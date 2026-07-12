using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class UserRepository(INeo4jContext context) : IUserRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<User?> GetByIdAsync(UserId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (u:User {Id: $id, TenantId: $tenantId}) RETURN u",
            new { id = id.Value.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<User?> GetByEmailAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (u:User {Email: $email, TenantId: $tenantId}) RETURN u",
            new { email, tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<User?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (u:User {Email: $email}) RETURN u LIMIT 1",
            new { email });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<User>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (u:User {TenantId: $tenantId}) RETURN u",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (u:User {
                Id: $id,
                TenantId: $tenantId,
                Email: $email,
                PasswordHash: $passwordHash,
                FirstName: $firstName,
                LastName: $lastName,
                Role: $role,
                IsActive: $isActive,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN u",
            new
            {
                id = user.Id.Value.ToString(),
                tenantId = user.TenantId.Value.ToString(),
                email = user.Email,
                passwordHash = user.PasswordHash,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role.ToString(),
                isActive = user.IsActive,
                createdAt = user.CreatedAt.ToString("O"),
                updatedAt = user.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (u:User {Id: $id, TenantId: $tenantId})
              SET u.Email = $email,
                  u.FirstName = $firstName,
                  u.LastName = $lastName,
                  u.Role = $role,
                  u.IsActive = $isActive,
                  u.UpdatedAt = $updatedAt",
            new
            {
                id = user.Id.Value.ToString(),
                tenantId = user.TenantId.Value.ToString(),
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role.ToString(),
                isActive = user.IsActive,
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(UserId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (u:User {Id: $id, TenantId: $tenantId}) DETACH DELETE u",
            new { id = id.Value.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static User MapToEntity(IRecord record)
    {
        var node = record["u"].As<INode>();
        var props = node.Properties;
        return new User
        {
            Id = new UserId(Guid.Parse(props["Id"].As<string>())),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            Email = props["Email"].As<string>(),
            PasswordHash = props["PasswordHash"].As<string>(),
            FirstName = props["FirstName"].As<string>(),
            LastName = props["LastName"].As<string>(),
            Role = Enum.Parse<UserRole>(props["Role"].As<string>()),
            IsActive = props["IsActive"].As<bool>(),
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>()),
            UpdatedAt = DateTime.Parse(props["UpdatedAt"].As<string>())
        };
    }
}
