using System.Collections.Concurrent;

namespace CardGameDBSites.API.Services
{
    public class ExportTokenService
    {
        private readonly record struct TokenEntry(int DeckId, Guid ExportId, DateTime ExpiresAt);

        private readonly ConcurrentDictionary<string, TokenEntry> _tokens = new();

        public string CreateToken(int deckId, Guid exportId, TimeSpan expiry)
        {
            PurgeExpired();
            var token = Guid.NewGuid().ToString("N");
            _tokens[token] = new TokenEntry(deckId, exportId, DateTime.UtcNow.Add(expiry));
            return token;
        }

        public bool TryConsume(string token, out int deckId, out Guid exportId)
        {
            deckId = 0;
            exportId = Guid.Empty;

            if (!_tokens.TryRemove(token, out var entry))
                return false;

            if (DateTime.UtcNow > entry.ExpiresAt)
                return false;

            deckId = entry.DeckId;
            exportId = entry.ExportId;
            return true;
        }

        private void PurgeExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var key in _tokens.Keys)
            {
                if (_tokens.TryGetValue(key, out var entry) && now > entry.ExpiresAt)
                    _tokens.TryRemove(key, out _);
            }
        }
    }
}
