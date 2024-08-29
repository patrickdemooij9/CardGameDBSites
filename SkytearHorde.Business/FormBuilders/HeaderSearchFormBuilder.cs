using FormBuilder.Core.Implementations.FormFields;
using FormBuilder.Core.Interfaces;

namespace SkytearHorde.Business.FormBuilders
{
    public class HeaderSearchFormBuilder : IFormHandler
    {
        public FormBuilder.Core.Common.FormBuilder.FormBuilder Create()
        {
            return new FormBuilder.Core.Common.FormBuilder.FormBuilder()
                .AddRow(row => row
                    .AddField<TextBoxField, string>(field => field
                        .SetProperty(it => it.Name, "Search")));
        }
    }
}
