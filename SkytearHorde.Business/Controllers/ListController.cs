using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.PostModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class ListController : SurfaceController
    {
        private readonly DeckListService _deckListService;
        private readonly IMemberManager _memberManager;

        public ListController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, DeckListService deckListService, IMemberManager memberManager) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _deckListService = deckListService;
            _memberManager = memberManager;
        }
        
        public async Task<IActionResult> HandleSubmit(AddToListPostModel postModel)
        {
            var currentUser = int.Parse((await _memberManager.GetCurrentMemberAsync()).Id);

            if (postModel.CreateNewList)
            {
                if (string.IsNullOrWhiteSpace(postModel.NewListName)) return RedirectToCurrentUmbracoUrl();
                _deckListService.CreateNewDeckList(postModel.NewListName, currentUser, new int[] { postModel.DeckId });
            }
            else
            {
                _deckListService.AddToList(postModel.ExistingListId, currentUser, postModel.DeckId);
            }

            return RedirectToCurrentUmbracoUrl();
        }

        public async Task<IActionResult> HandleEdit([Bind(Prefix = "PostModel")] EditListPostModel postModel)
        {
            var list = _deckListService.Get(postModel.Id);
            var currentUser = int.Parse((await _memberManager.GetCurrentMemberAsync()).Id);
            if (list is null || currentUser != list.CreatedBy) return Redirect("/");

            list.Name = postModel.Name;
            list.Description = postModel.Description;
            list.DeckIds = postModel.DeckItems.Where(it => it.Enabled).Select(it => it.DeckId).ToList();

            _deckListService.Update(list);

            return Redirect($"/list/{postModel.Id}");
        }
    }
}
