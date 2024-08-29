using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SkytearHorde.Business.BackgroundRunners;
using SkytearHorde.Business.Processors;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Business.Middleware
{
    public class TrackDeckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DeckTrackingProcessor _deckTrackingProcessor;

        private readonly string[] _deckSegments = new [] { "decks", "strike-teams" };

        public TrackDeckMiddleware(RequestDelegate next,
            DeckTrackingProcessor deckTrackingProcessor,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _next = next;
            _deckTrackingProcessor = deckTrackingProcessor;
        }

        public async Task Invoke(HttpContext context)
        {
            var url = new Uri(context.Request.GetEncodedUrl());

            if (url.Segments.Length != 3 || !_deckSegments.Contains(url.Segments[1].TrimEnd('/')))
            {
                await _next.Invoke(context);
                return;
            }

            var deckIdString = url.Segments[2];
            if (int.TryParse(deckIdString, out var deckId))
            {
                _deckTrackingProcessor.AddDeckView(deckId);
            }
            await _next.Invoke(context);
        }
    }
}
