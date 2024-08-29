using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Repository;
using SkytearHorde.Entities.Models.ViewModels;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.ViewComponents
{
    public class DeckOverviewDataViewComponent : ViewComponent
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly DeckService _deckService;
        private readonly MemberInfoService _memberInfoService;

        public DeckOverviewDataViewComponent(IUmbracoContextFactory umbracoContextFactory, DeckService deckService, MemberInfoService memberInfoService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _deckService = deckService;
            _memberInfoService = memberInfoService;
        }

        public IViewComponentResult Invoke(OverviewDataViewModel model)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var overviewPage = ctx.UmbracoContext.Content?.GetById(model.PageId) as DeckOverview ?? throw new ArgumentException("Could not find deckoverview page");

            var page = model.PageNumber ?? 1;

            var selectedCards = model.Config.Filters.FirstOrDefault(it => it.Alias == "Cards")?.Items
                .Select(it => it.Value)
                .Select(it => int.TryParse(it, out var result) ? result : (int?)null)
                .Where(it => it != null)
                .Cast<int>()
                .ToArray() ?? Array.Empty<int>();

            var loggedInMember = _memberInfoService.GetMemberInfo()?.Id;
            var deckTypeId = (overviewPage.SquadSettings as SquadSettings)?.TypeID ?? 1;
            var request = new DeckPagedRequest(deckTypeId)
            {
                Cards = selectedCards,
                Page = page,
                Take = overviewPage.DecksPerRow * 3,
                OrderBy = model.SortBy.IfNullOrWhiteSpace("popular"),
                UseUserCollectionId = model.SortBy?.Equals("collection") is true ? loggedInMember : null
            };
            var decks = _deckService.GetAll(request);

            var viewModel = new DeckOverviewDataModel(decks)
            {
                DecksPerRow = overviewPage.DecksPerRow,
                Page = page,
                BaseUrl = overviewPage.Url()
            };
            return View("/Views/Partials/components/deckOverviewData.cshtml", viewModel);
        }
    }
}
