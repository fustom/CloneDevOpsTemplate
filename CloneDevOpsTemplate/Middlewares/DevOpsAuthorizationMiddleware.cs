using CloneDevOpsTemplate.Services;

namespace CloneDevOpsTemplate.Middlewares
{
    public class DevOpsAuthorizationMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value is not null && context.Request.Path.Value.Contains("/Home/Login"))
            {
                await _next(context);
                return;
            }

            if (context.Session.GetString(Const.SessionKeyOrganizationName) == null || context.Session.GetString(Const.SessionKeyAccessToken) == null)
            {
                context.Response.Redirect("/Home/Login");
                return;
            }

            await _next(context);
        }
    }
}