using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Helpers
{
    public class ViewRenderHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private readonly ICompositeViewEngine _viewEngine;

        public ViewRenderHelper(IHttpContextAccessor httpContextAccessor,
            IModelMetadataProvider modelMetadataProvider,
            ITempDataDictionaryFactory tempDataDictionaryFactory,
            ICompositeViewEngine viewEngine)
        {
            _httpContextAccessor = httpContextAccessor;
            _modelMetadataProvider = modelMetadataProvider;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _viewEngine = viewEngine;
        }

        public HtmlString RenderView(string viewName, object model)
        {
            var httpContext = _httpContextAccessor.GetRequiredHttpContext();

            // isMainPage is set to true here to ensure ViewStart(s) found in the view hierarchy are rendered
            var viewResult = _viewEngine.GetView(null, viewName, isMainPage: true);

            if (viewResult.Success == false)
            {
                throw new InvalidOperationException($"A view with the name {viewName} could not be found");
            }

            var viewData = new ViewDataDictionary(_modelMetadataProvider, new ModelStateDictionary())
            {
                Model = model
            };

            var writer = new StringWriter();
            var viewContext = new ViewContext(
                new ActionContext(httpContext, httpContext.GetRouteData(), new ControllerActionDescriptor()),
                viewResult.View,
                viewData,
                _tempDataDictionaryFactory.GetTempData(httpContext),
                writer,
                new HtmlHelperOptions()
            );


            viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();

            return new HtmlString(writer.GetStringBuilder().ToString());
        }
    }
}
