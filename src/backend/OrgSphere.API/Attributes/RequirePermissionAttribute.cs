using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrgSphere.Domain.Interfaces;

namespace OrgSphere.API.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute(string module, string action, string scope = "own") : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantContext = context.HttpContext.RequestServices.GetService(typeof(ITenantContext)) as ITenantContext;

        if (tenantContext?.Role is null || !tenantContext.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Authentication required" });
            return;
        }

        var authService = context.HttpContext.RequestServices.GetRequiredService<OrgSphere.Application.Services.IAuthorizationService>();

        if (!authService.HasPermission(tenantContext.Role.Value, module, action, scope))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
