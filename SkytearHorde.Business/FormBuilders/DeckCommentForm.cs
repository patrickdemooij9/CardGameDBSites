using System.ComponentModel.DataAnnotations;

namespace SkytearHorde.Business.FormBuilders
{
    public class DeckCommentForm
    {
        public int SourceId { get; set; }
        [Required]
        public string Comment { get; set; }
    }
}
