using Microsoft.AspNetCore.Http;
using System.Text;

namespace SkytearHorde.Business.Middleware
{
    public class CountryMiddleware
    {
        public const string CountrySession = "Country";

        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;

        public CountryMiddleware(RequestDelegate next, HttpClient httpClient)
        {
            _next = next;
            _httpClient = httpClient;
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsValidPage(context) && !context.Session.Keys.Contains(CountrySession))
            {
                await SetSessionCountry(context);
            }

            await _next.Invoke(context);
        }

        private bool IsValidPage(HttpContext context)
        {
            return !Path.HasExtension(context.Request.Path);
        }

        private async Task SetSessionCountry(HttpContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.Request.Query["country"].ToString()))
            {
                context.Session.SetString(CountrySession, context.Request.Query["country"].ToString());
                return;
            }

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ip))
            {
                context.Session.SetString(CountrySession, "UNKNOWN");
                return;
            }
            try
            {
                var resultString = await _httpClient.GetStringAsync($"https://api.hostip.info/get_html.php?ip={ip}");
                var country = resultString.Substring(resultString.IndexOf('(') + 1, 2);
                context.Session.SetString(CountrySession, country);
            }
            catch
            {
                context.Session.SetString(CountrySession, "UNKNOWN");
            }
        }
    }
}
