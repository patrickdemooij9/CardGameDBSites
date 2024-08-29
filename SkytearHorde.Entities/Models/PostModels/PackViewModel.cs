using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;

namespace SkytearHorde.Entities.Models.PostModels
{
	public class PackViewModel : FormPostModel<PackPostModel>
	{
		public SetViewModel[] Sets { get; set; } = [];
        public VariantType[] VariantTypes { get; set; } = [];
	}
}
