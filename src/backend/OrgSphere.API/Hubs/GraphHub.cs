using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.API.Hubs;

[Authorize]
public class GraphHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = GetTenantId();
        if (tenantId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = GetTenantId();
        if (tenantId is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private TenantId? GetTenantId()
    {
        var claim = Context.User?.FindFirst("tenantId")
                    ?? Context.User?.FindFirst(ClaimTypes.GroupSid);
        if (claim is not null && Guid.TryParse(claim.Value, out var tenantId))
        {
            return new TenantId(tenantId);
        }

        return null;
    }
}
