using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Entities.Models.PostModels;
using Umbraco.Cms.Web.Common.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class MarkdownController : UmbracoApiController
    {
        [HttpPost]
        public string Preview(MarkdownPreviewPostModel model)
        {
            return MarkdownHelper.ToHtml(model.Markdown);
        }
    }
}
