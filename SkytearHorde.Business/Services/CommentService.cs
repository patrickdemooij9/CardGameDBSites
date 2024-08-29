using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services
{
    public class CommentService
    {
        private readonly DeckCommentRepository _deckCommentRepository;
        private readonly CardCommentRepository _cardCommentRepository;
        private readonly ISiteAccessor _siteAccessor;

        public CommentService(DeckCommentRepository deckCommentRepository,
            CardCommentRepository cardCommentRepository,
            ISiteAccessor siteAccessor)
        {
            _deckCommentRepository = deckCommentRepository;
            _cardCommentRepository = cardCommentRepository;
            _siteAccessor = siteAccessor;
        }

        public DeckComment[] GetByDeck(int deckId)
        {
            return _deckCommentRepository.GetByDeck(deckId);
        }

        public CardComment[] GetByCard(int cardId)
        {
            return _cardCommentRepository.GetByCard(cardId);
        }

        public DeckComment AddNewDeckComment(int deckId, string comment, int createdBy, int? parentId = null)
        {
            var model = new DeckComment
            {
                DeckId = deckId,
                Comment = comment,
                SiteId = _siteAccessor.GetSiteId(),
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                ParentId = parentId
            };
            var id = _deckCommentRepository.Add(model);
            return _deckCommentRepository.Get(id)!;
        }

        public DeckComment? GetDeckComment(int id)
        {
            return _deckCommentRepository.Get(id);
        }

        public void DeleteDeckComment(int commentId)
        {
            var comment = _deckCommentRepository.Get(commentId);
            if (comment is null) return;

            _deckCommentRepository.Delete(comment);
        }

        public CardComment AddNewCardComment(int cardId, string comment, int createdBy, int? parentId = null)
        {
            var model = new CardComment
            {
                CardId = cardId,
                Comment = comment,
                SiteId = _siteAccessor.GetSiteId(),
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                ParentId = parentId
            };
            var id = _cardCommentRepository.Add(model);
            return _cardCommentRepository.Get(id)!;
        }

        public CardComment? GetCardComment(int id)
        {
            return _cardCommentRepository.Get(id);
        }

        public void DeleteCardComment(int commentId)
        {
            var comment = _cardCommentRepository.Get(commentId);
            if (comment is null) return;

            _cardCommentRepository.Delete(comment);
        }

        public IComment[] GetLatest(int amount)
        {
            var comments = new List<IComment>();
            comments.AddRange(_cardCommentRepository.GetLatest(amount));
            comments.AddRange(_deckCommentRepository.GetLatest(amount));
            return comments.OrderByDescending(it => it.CreatedAt).Take(amount).ToArray();
        }
    }
}
