using DeviceDetectorNET;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Business;
using System.Collections.Concurrent;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Processors
{
    public class ViewTrackingProcessor
    {
        private readonly ViewSessionGenerator _viewSessionGenerator;
        private readonly IScopeProvider _scopeProvider;
        private readonly ConcurrentQueue<PageView> _viewsToSync;

        public ViewTrackingProcessor(ViewSessionGenerator viewSessionGenerator, IScopeProvider scopeProvider)
        {
            _viewSessionGenerator = viewSessionGenerator;
            _scopeProvider = scopeProvider;
            _viewsToSync = new ConcurrentQueue<PageView>();
        }

        public void TrackView(HttpContext httpContext)
        {
            if (new DeviceDetector(httpContext.Request.Headers["User-Agent"].ToString()).IsBot()) return;

            var sessionId = _viewSessionGenerator.GetSessionId(httpContext);

            var pageView = new PageView
            {
                Url = httpContext.Request.GetEncodedUrl(),
                SessionId = sessionId,
                VisitedDate = DateTime.UtcNow
            };

            _viewsToSync.Enqueue(pageView);
        }

        public void SyncViews()
        {
            var copied = new Queue<PageView>(_viewsToSync);
            _viewsToSync.Clear();

            using var scope = _scopeProvider.CreateScope();
            scope.Database.InsertBulk(copied.WhereNotNull().Select(it => new PageView
            {
                Url = it.Url,
                SessionId = it.SessionId,
                VisitedDate = it.VisitedDate
            }));
            scope.Complete();
        }
    }
}
