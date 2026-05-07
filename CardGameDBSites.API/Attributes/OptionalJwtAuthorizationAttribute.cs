using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Umbraco.Extensions;

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
                var identity = new ClaimsIdentity(
                    result.Principal.Claims,
                    IdentityConstants.ApplicationScheme
                    );

                httpContext.User = new ClaimsPrincipal(identity);
            }
        }
    }
}
