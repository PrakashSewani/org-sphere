using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrgSphere.Domain;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.API.Middleware;

public class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (tenantContext is TenantContext ctx && context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenantId") ?? context.User.FindFirst(ClaimTypes.GroupSid);
            if (tenantIdClaim is not null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                ctx.TenantId = new TenantId(tenantId);
            }

            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                ctx.UserId = new UserId(userId);
            }

            var roleClaim = context.User.FindFirst("role") ?? context.User.FindFirst(ClaimTypes.Role);
            if (roleClaim is not null && Enum.TryParse<UserRole>(roleClaim.Value, out var role))
            {
                ctx.Role = role;
            }
        }

        await next(context);
    }
}
