using Moq;
using OrgSphere.Domain;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Tests.Fixtures;

public class MockTenantContextFixture : ITenantContext
{
    public TenantId? TenantId { get; set; }
    public UserId? UserId { get; set; }
    public UserRole? Role { get; set; }
    public bool IsAuthenticated => TenantId is not null && UserId is not null;

    public void SetTenantContext(TenantId tenantId, UserId userId, UserRole role)
    {
        TenantId = tenantId;
        UserId = userId;
        Role = role;
    }
}

public static class MockEventBusFixture
{
    public static Mock<IEventBus> Create()
    {
        var mock = new Mock<IEventBus>();
        mock.Setup(e => e.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}
