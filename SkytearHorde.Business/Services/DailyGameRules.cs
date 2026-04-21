namespace SkytearHorde.Business.Services
{
    public static class DailyGameRules
    {
        public static string ResolveStatus(bool isCorrect, int attemptsUsed, int maxAttempts)
        {
            if (isCorrect)
            {
                return "Solved";
            }

            return attemptsUsed >= maxAttempts ? "Failed" : "InProgress";
        }

        public static string CompareValues(string[] targetValues, string[] guessValues)
        {
            if (targetValues.Length == 0 && guessValues.Length == 0)
            {
                return "unknown";
            }

            if (targetValues.Length == 1 && guessValues.Length == 1
                && int.TryParse(targetValues[0], out var targetNumber)
                && int.TryParse(guessValues[0], out var guessNumber))
            {
                if (targetNumber == guessNumber) return "exact";
                return guessNumber < targetNumber ? "higher" : "lower";
            }

            var targetSet = targetValues.Select(it => it.ToLowerInvariant()).ToHashSet();
            var guessSet = guessValues.Select(it => it.ToLowerInvariant()).ToHashSet();

            if (targetSet.SetEquals(guessSet))
            {
                return "exact";
            }

            if (targetSet.Overlaps(guessSet))
            {
                return "partial";
            }

            return "none";
        }

        public static IEnumerable<T> Rank<T>(IEnumerable<T> entries, Func<T, bool> solvedSelector, Func<T, int> attemptsSelector, Func<T, int> elapsedSecondsSelector)
        {
            return entries
                .OrderBy(it => solvedSelector(it) ? 0 : 1)
                .ThenBy(attemptsSelector)
                .ThenBy(elapsedSecondsSelector);
        }
    }
}
