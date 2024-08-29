using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CommentComponentViewModel
    {
        public IComment[] Comments { get; set; }
        public string ControllerName { get; set; }
        public int SourceId { get; set; }

        public CommentComponentViewModel(string controllerName, int sourceId)
        {
            Comments = Array.Empty<IComment>();

            ControllerName = controllerName;
            SourceId = sourceId;
        }
    }
}
