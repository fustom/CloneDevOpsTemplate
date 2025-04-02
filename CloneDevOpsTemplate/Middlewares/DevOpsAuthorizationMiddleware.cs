using CloneDevOpsTemplate.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace CloneDevOpsTemplate.Middlewares;

public class DevOpsAuthorizationMiddleware : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (context.Session.GetString(Const.SessionKeyOrganizationName) == null || context.Session.GetString(Const.SessionKeyAccessToken) == null)
        {
            context.Response.Redirect("/Home/Login");
            return;
        }

        await next(context);
    }
}