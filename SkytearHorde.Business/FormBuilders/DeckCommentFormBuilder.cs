using FormBuilder.Core.Implementations.FormFields;
using FormBuilder.Core.Interfaces;

namespace SkytearHorde.Business.FormBuilders
{
    public class DeckCommentFormBuilder : IFormHandler
    {
        public FormBuilder.Core.Common.FormBuilder.FormBuilder Create()
        {
            return new FormBuilder.Core.Common.FormBuilder.FormBuilder()
                .AddRow(it => it
                    .AddField<TextAreaField, string>(field => field.
                        FromObjectProperty<DeckCommentForm>(m => m.Comment)
                        )
                    );
        }
    }
}
