using CardGameDBSites.API.Attributes;
using CardGameDBSites.API.Models.CardLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/cardlists")]
    [JwtAuthorization]
    [ApiExplorerSettings(GroupName = "CardLists")]
    public class CardListApiController : Controller
    {
        private readonly CardListService _cardListService;
        private readonly MemberInfoService _memberInfoService;

        public CardListApiController(CardListService cardListService, MemberInfoService memberInfoService)
        {
            _cardListService = cardListService;
            _memberInfoService = memberInfoService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(CardListApiModel[]), 200)]
        public IActionResult GetLists()
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            _cardListService.EnsureDefaultListExists(memberId.Value);

            var lists = _cardListService.GetByUser(memberId.Value);
            return Ok(lists.Select(it => new CardListApiModel
            {
                Id = it.Id,
                Name = it.Name,
                Description = it.Description,
                IsPublic = it.IsPublic,
                ItemCount = it.Items.Count,
                CreatedDate = it.CreatedDate
            }).ToArray());
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(CardListApiModel), 200)]
        public IActionResult CreateList([FromBody] CreateCardListPostModel model)
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required");

            var id = _cardListService.Create(model.Name, memberId.Value, model.Description, model.IsPublic);
            return Ok(new CardListApiModel
            {
                Id = id,
                Name = model.Name,
                Description = model.Description,
                IsPublic = model.IsPublic,
                ItemCount = 0,
                CreatedDate = DateTime.UtcNow
            });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        public IActionResult UpdateList(int id, [FromBody] UpdateCardListPostModel model)
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required");

            var success = _cardListService.Update(id, memberId.Value, model.Name, model.Description, model.IsPublic);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        public IActionResult DeleteList(int id)
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            var success = _cardListService.Delete(id, memberId.Value);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CardListDetailApiModel), 200)]
        public IActionResult GetList(int id)
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            var list = _cardListService.Get(id);
            if (list is null || list.CreatedBy != memberId.Value) return NotFound();

            return Ok(MapToDetail(list));
        }

        [HttpGet("{id}/public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CardListDetailApiModel), 200)]
        public IActionResult GetPublicList(int id)
        {
            var list = _cardListService.GetPublic(id);
            if (list is null) return NotFound();

            return Ok(MapToDetail(list));
        }

        [HttpPost("{id}/items")]
        [ProducesResponseType(typeof(CardListItemApiModel), 200)]
        public IActionResult AddItem(int id, [FromBody] AddCardListItemPostModel model)
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            if (model.Amount < 1) return BadRequest("Amount must be at least 1");

            var item = _cardListService.AddItem(id, memberId.Value, model.CardId, model.VariantId, model.Amount);
            if (item is null) return NotFound();

            return Ok(new CardListItemApiModel
            {
                Id = item.Id,
                CardId = item.CardId,
                VariantId = item.VariantId,
                Amount = item.Amount,
                AddedDate = item.AddedDate
            });
        }

        [HttpDelete("{id}/items/{itemId}")]
        [ProducesResponseType(200)]
        public IActionResult RemoveItem(int id, int itemId)
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            var success = _cardListService.RemoveItem(id, memberId.Value, itemId);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpPost("cardStatus")]
        [ProducesResponseType(typeof(CardInListApiModel[]), 200)]
        public IActionResult GetCardStatus([FromBody] int cardId)
        {
            var memberId = GetMemberId();
            if (memberId is null) return Unauthorized();

            var items = _cardListService.GetItemsByCard(cardId, memberId.Value);
            return Ok(items.Select(it => new CardInListApiModel
            {
                ListId = it.ListId,
                ItemId = it.Id,
                VariantId = it.VariantId,
                Amount = it.Amount
            }).ToArray());
        }

        private CardListDetailApiModel MapToDetail(SkytearHorde.Entities.Models.Business.CardList list)
        {
            var ownerName = _memberInfoService.GetName(list.CreatedBy);

            return new CardListDetailApiModel
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                CreatedDate = list.CreatedDate,
                OwnerName = ownerName,
                Items = list.Items.OrderByDescending(it => it.AddedDate).Select(it => new CardListItemApiModel
                {
                    Id = it.Id,
                    CardId = it.CardId,
                    VariantId = it.VariantId,
                    Amount = it.Amount,
                    AddedDate = it.AddedDate
                }).ToArray()
            };
        }

        private int? GetMemberId()
        {
            var member = _memberInfoService.GetMemberInfo();
            return member?.Id;
        }
    }
}
