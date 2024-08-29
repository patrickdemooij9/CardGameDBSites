using Microsoft.AspNetCore.Http;
using System.Net;

namespace SkytearHorde.Business.Middleware
{
    public class MetricsAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public MetricsAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (authHeader.Equals("GrafanaMetrics"))
            {
                await _next.Invoke(context);
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
