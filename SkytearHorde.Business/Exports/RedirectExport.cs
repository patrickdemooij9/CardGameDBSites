using SkytearHorde.Entities.Models.Business;
using System.Text;

namespace SkytearHorde.Business.Exports
{
    public class RedirectExport : IDeckExport
    {
        private readonly string _url;

        public RedirectExport(string url)
        {
            _url = url;
        }

        public Task<byte[]> ExportDeck(Deck deck)
        {
            var url = _url.Replace("{id}", deck.Id.ToString());
            return Task.FromResult(Encoding.UTF8.GetBytes(url));
        }
    }
}
