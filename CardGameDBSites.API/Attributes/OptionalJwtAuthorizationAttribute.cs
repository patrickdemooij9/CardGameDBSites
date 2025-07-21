using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CardGameDBSites.API.Attributes
{
    public class OptionalJwtAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext.User?.Identity?.IsAuthenticated is true)
            {
                return;
            }

            var result = await httpContext.AuthenticateAsync("Jwt");
            if (result?.Succeeded == true)
            {
                httpContext.User = result.Principal;
            }
        }
    }
}
