using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Business.Extensions
{
    public static class OverviewFilterExtensions
    {
        public static string GetFilterAlias(this OverviewFilter filter)
        {
            if (filter.IsExpansionFilter) return "Set Name";
            if (!string.IsNullOrWhiteSpace(filter.ExamineField)) return filter.ExamineField;
            return filter.Ability!.Name;
        }
    }
}
