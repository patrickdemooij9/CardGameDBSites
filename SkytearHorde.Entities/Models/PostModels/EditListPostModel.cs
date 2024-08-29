using System.ComponentModel.DataAnnotations;

namespace SkytearHorde.Entities.Models.PostModels
{
	public class EditListPostModel
	{
		[Required]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		public string? Description { get; set; }

		public EditListItemPostModel[] DeckItems { get; set; }

		public EditListPostModel()
		{
			DeckItems = Array.Empty<EditListItemPostModel>();
		}
	}
}
