namespace SkytearHorde.Entities.Models.ViewModels
{
    public class AjaxResponse
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public bool IsInner { get; set; }

        public AjaxResponse(string id, string content)
        {
            Id = id;
            Content = content;
            IsInner = true;
        }
    }
}
