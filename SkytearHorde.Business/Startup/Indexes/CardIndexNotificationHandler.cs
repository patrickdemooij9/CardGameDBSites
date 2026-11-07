using Examine;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Indexes
{
    internal class CardIndexNotificationHandler : INotificationHandler<ContentCacheRefresherNotification>
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
        private readonly IExamineManager _examineManager;
        private readonly IContentService _contentService;
        private readonly CardIndexValueSetBuilder _cardIndexValueSetBuilder;
        private readonly CardService _cardService;
        private readonly DeckLegalityService _deckLegalityService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public CardIndexNotificationHandler(
            IRuntimeState runtimeState,
            IUmbracoIndexingHandler umbracoIndexingHandler,
            IExamineManager examineManager,
            IContentService contentService,
            CardIndexValueSetBuilder cardIndexValueSetBuilder,
            CardService cardService,
            DeckLegalityService deckLegalityService,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _runtimeState = runtimeState;
            _umbracoIndexingHandler = umbracoIndexingHandler;
            _examineManager = examineManager;
            _contentService = contentService;
            _cardIndexValueSetBuilder = cardIndexValueSetBuilder;
            _cardService = cardService;
            _deckLegalityService = deckLegalityService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        /// <summary>
        ///     Updates the index based on content changes.
        /// </summary>
        public void Handle(ContentCacheRefresherNotification notification)
        {
            if (NotificationHandlingIsDisabled())
            {
                return;
            }

            if (!_examineManager.TryGetIndex("CardIndex", out IIndex? index))
            {
                throw new InvalidOperationException("Could not obtain the product index");
            }

            ContentCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads(notification);

            foreach (ContentCacheRefresher.JsonPayload payload in payloads)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    index.DeleteFromIndex(payload.Id.ToString());
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode) ||
                         payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                {
                    var baseCard = _cardService.Get(payload.Id);
                    if (baseCard != null)
                    {
                        var variants = _cardService.GetVariants(payload.Id).ToArray();
                        index.IndexItems(_cardIndexValueSetBuilder.GetValueSets(variants));
                        continue;
                    }

                    var card = _cardService.GetVariant(payload.Id);
                    if (card is null)
                    {
                        index.DeleteFromIndex(payload.Id.ToString());
                        ReindexCardsWhenSetLegalityChanged(payload.Id);
                        continue;
                    }

                    index.IndexItems(_cardIndexValueSetBuilder.GetValueSets(card));
                }
            }
        }

        private void ReindexCardsWhenSetLegalityChanged(int contentId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            if (ctx.UmbracoContext.Content?.GetById(contentId) is Set set)
            {
                _deckLegalityService.ReindexCardsWhenSetLegalityChanged(set);
            }
        }

        private bool NotificationHandlingIsDisabled()
        {
            // Only handle events when the site is running.
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return true;
            }

            if (_umbracoIndexingHandler.Enabled == false)
            {
                return true;
            }

            if (Suspendable.ExamineEvents.CanIndex == false)
            {
                return true;
            }

            return false;
        }

        private ContentCacheRefresher.JsonPayload[] GetNotificationPayloads(CacheRefresherNotification notification)
        {
            if (notification.MessageType != MessageType.RefreshByPayload ||
                notification.MessageObject is not ContentCacheRefresher.JsonPayload[] payloads)
            {
                throw new NotSupportedException();
            }

            return payloads;
        }
    }
}
