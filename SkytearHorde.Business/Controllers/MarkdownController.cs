using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.Business.Controllers
{
    [ApiController]
    [Route("umbraco/api/markdown/[action]")]
    public class MarkdownController : ControllerBase
    {
        [HttpPost]
        public string Preview(MarkdownPreviewPostModel model)
        {
            return MarkdownHelper.ToHtml(model.Markdown);
        }
    }
}
