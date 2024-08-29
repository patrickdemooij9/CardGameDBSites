using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SkytearHorde.Entities.Models.ViewModels;

namespace SkytearHorde.Business.TagHelpers
{
    [HtmlTargetElement("c-button")]
    public class ButtonTagHelper : TagHelper
    {
        public ButtonSize Size { get; set; }
        public bool IsLink { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = IsLink ? "a" : "button";

            string classes = "flex justify-center align-center ";
            switch (Size)
            {
                case ButtonSize.Small:
                    classes += "p-2 bg-main-color text-white rounded hover:bg-main-color-hover";
                    break;
                default:
                case ButtonSize.Medium:
                    classes += "px-4 py-2 bg-main-color text-white rounded hover:bg-main-color-hover";
                    break;
                case ButtonSize.Large:
                    classes += "";
                    break;
            }
            var tagBuilder = new TagBuilder(IsLink ? "a" : "button");
            tagBuilder.Attributes.Add("class", classes);

            output.MergeAttributes(tagBuilder);
        }
    }
}
