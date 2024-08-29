using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SkytearHorde.Business.Processors;

namespace SkytearHorde.Business.Middleware
{
    public class TrackViewMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ViewTrackingProcessor _viewTrackingProcessor;

        public TrackViewMiddleware(RequestDelegate next, ViewTrackingProcessor viewTrackingProcessor)
        {
            _next = next;
            _viewTrackingProcessor = viewTrackingProcessor;
        }

        public async Task Invoke(HttpContext context)
        {
            var url = new Uri(context.Request.GetEncodedUrl());

            if (url.Segments.Contains("/umbraco") ||
                url.Segments.Contains("umbraco") ||
                Path.HasExtension(context.Request.Path.Value))
            {
                await _next.Invoke(context);
                return;
            }

            _viewTrackingProcessor.TrackView(context);
            await _next.Invoke(context);
        }
    }
}
