namespace SkytearHorde.Business.Services.Search
{
    public class CardSorting
    {
        public string Field { get; set; }
        public bool IsDescending { get; set; }

        public CardSorting(string field)
        {
            Field = field;
        }
    }
}
