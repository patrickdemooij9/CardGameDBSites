using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.PostModels
{
    public abstract class FormPostModel<T> where T : new()
    {
        public string? ErrorMessage { get; set; }
        public T PostContent { get; set; }

        public FormPostModel()
        {
            PostContent = new T();
        }

        public FormPostModel(T postContent)
        {
            PostContent = postContent;
        }
    }
}
