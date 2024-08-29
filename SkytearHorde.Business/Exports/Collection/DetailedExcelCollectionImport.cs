using OfficeOpenXml.Table;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;

namespace SkytearHorde.Business.Exports.Collection
{
    public class DetailedExcelCollectionImport : IExcelCollectionImport
    {
        private readonly VariantTypeViewModel[] _variants;
        private readonly CardService _cardService;

        public DetailedExcelCollectionImport(VariantTypeViewModel[] variants, CardService cardService)
        {
            _variants = variants;
            _cardService = cardService;
        }

        public bool CanImport(ExcelTable table)
        {
            var columnNamesToMatch = new string[]
            {
                "Base card id",
                "Set",
                "Standard Count",
                "Foil Count"
            };

            var tableColumnNames = table.Columns.Select(it => it.Name).Distinct().ToArray();
            return columnNamesToMatch.All(it => tableColumnNames.Contains(it));
        }

        public CollectionCardItem[] Import(ExcelTable table)
        {
            var items = new List<CollectionCardItem>();
            var allCards = _cardService.GetAll(true)
                .Where(it => it.VariantId > 0)
                .GroupBy(it => it.GetMultipleCardAttributeValue("SWU Id")!.First())
                .ToDictionary(it => it.Key, it => it.ToArray());

            var start = table.Address.Start;
            var end = table.Address.End;

            var setColumn = table.Columns.First(it => it.Name == "Set");
            var cardNumberColumn = table.Columns.First(it => it.Name == "Base card id");
            var countColumn = table.Columns.First(it => it.Name == "Standard Count");
            var foilCountColumn = table.Columns.First(it => it.Name == "Foil Count");

            var allSets = _cardService.GetAllSets().ToDictionary(it => it.SetCode!.ToLowerInvariant(), it => it.Id);
            var foilVariantTypes = _variants.Where(it => it.ChildOfBase || it.ChildOf.HasValue).Select(it => it.Id).ToArray();

            for (var r = start.Row + 1; r <= end.Row; r++)
            {
                var item = new CollectionCardItem();

                var cardIdString = table.WorkSheet.Cells[r, cardNumberColumn.Position + 1].Value?.ToString();

                if (!int.TryParse(cardIdString, out var cardId) || !allCards.ContainsKey(cardId.ToString("000")))
                {
                    throw new Exception($"Could not find card with ID: {cardIdString}");
                }

                var setCode = table.WorkSheet.Cells[r, setColumn.Position + 1].Value?.ToString();

                if (string.IsNullOrWhiteSpace(setCode) || !allSets.TryGetValue(setCode.ToLowerInvariant(), out var setId))
                {
                    throw new Exception($"No set exists with code: {setCode}");
                }

                var cards = allCards[cardId.ToString("000")].Where(it => it.SetId == setId).ToArray();

                if (cards.Length == 0)
                {
                    throw new Exception($"Could not find card with ID: {cardIdString}");
                }

                var countString = table.WorkSheet.Cells[r, countColumn.Position + 1].Value?.ToString();
                if (int.TryParse(countString, out var count) && count > 0)
                {
                    var card = cards.FirstOrDefault(it => it.VariantTypeId is null) ?? cards.FirstOrDefault(it => !foilVariantTypes.Contains(it.VariantTypeId ?? 0)) ?? throw new Exception($"Could not find card with ID: {cardIdString}");

                    items.Add(new CollectionCardItem
                    {
                        CardId = card.BaseId,
                        VariantId = card.VariantId,
                        Amount = count,
                    });
                }

                var foilCountString = table.WorkSheet.Cells[r, foilCountColumn.Position + 1].Value?.ToString();

                if (int.TryParse(foilCountString, out var foilCount) && foilCount > 0)
                {
                    var card = cards.FirstOrDefault(it => foilVariantTypes.Contains(it.VariantTypeId ?? 0)) ?? throw new Exception($"Could not find card with ID: {cardIdString}");

                    //TODO: Variant types currently still take it from base instead of baseon type

                    items.Add(new CollectionCardItem
                    {
                        CardId = card.BaseId,
                        VariantId = card.VariantId,
                        Amount = count,
                    });
                }
            }

            return [.. items];
        }
    }
}
