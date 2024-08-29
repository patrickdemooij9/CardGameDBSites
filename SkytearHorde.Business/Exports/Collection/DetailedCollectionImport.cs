using Markdig.Extensions.Tables;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;
using System.Linq;
using static Umbraco.Cms.Core.Constants.HttpContext;

namespace SkytearHorde.Business.Exports.Collection
{
    public class DetailedCollectionImport
    {
        private readonly VariantTypeViewModel[] _variants;
        private readonly CardService _cardService;

        public DetailedCollectionImport(VariantTypeViewModel[] variants, CardService cardService)
        {
            _variants = variants;
            _cardService = cardService;
        }

        public Task<byte[]> Export(CollectionCardItem[] collection)
        {
            using var excelPackage = new ExcelPackage();
            var worksheet = excelPackage.Workbook.Worksheets.Add("Data");

            worksheet.Cells[1, 1].Value = "Set";
            worksheet.Cells[1, 2].Value = "CardNumber";
            worksheet.Cells[1, 3].Value = "Count";
            worksheet.Cells[1, 4].Value = "IsFoil";

            var cards = _cardService.GetAll();
            var collectionItems = collection.GroupBy(it => it.CardId).ToDictionary(it => it.Key, it => it);

            var row = 2;
            foreach (var card in _cardService.GetAll().OrderBy(it => it.GetMultipleCardAttributeValue("SWU Id")?.FirstOrDefault()))
            {
                collectionItems.TryGetValue(card.BaseId, out var group);
                var variantPages = _cardService.GetVariants(card.BaseId).ToDictionary(it => it.VariantTypeId ?? 0, it => it);

                var baseId = card.GetMultipleCardAttributeValue("SWU Id")?.FirstOrDefault();

                var baseCard = variantPages[0];
                var baseAmount = group?.FirstOrDefault(it => it.VariantId == baseCard.VariantId);
                if (baseAmount != null)
                {
                    worksheet.Cells[row, 1].Value = card.SetName;
                    worksheet.Cells[row, 2].Value = baseId;
                    worksheet.Cells[row, 3].Value = baseAmount?.Amount ?? 0;
                    worksheet.Cells[row, 4].Value = false;
                    row++;
                }

                foreach (var variant in _variants)
                {
                    var variantAmount = group?.FirstOrDefault(it => it.VariantId == variant.Id);
                    if (variantAmount is null) continue;

                    var variantPage = variantPages.TryGetValue(variant.Id, out Card? value) ? value : null;

                    worksheet.Cells[row, 1].Value = card.SetName;
                    worksheet.Cells[row, 2].Value = variantPage?.GetMultipleCardAttributeValue("SWU Id")?.FirstOrDefault() ?? baseId;
                    worksheet.Cells[row, 3].Value = variantAmount.Amount;
                    worksheet.Cells[row, 4].Value = variantPage is null;

                    row++;
                }
            }
            worksheet.Tables.Add(new ExcelAddressBase(1, 1, row - 1, 3), "Collection");

            return Task.FromResult(excelPackage.GetAsByteArray());
        }

        public bool CanImport(ExcelTable table)
        {
            var columnNamesToMatch = new string[]
            {
                "Set",
                "CardNumber",
                "Count",
                "IsFoil"
            };

            var tableColumnNames = table.Columns.Select(it => it.Name).Distinct().ToArray();
            return columnNamesToMatch.All(it => tableColumnNames.Contains(it));
        }

        /*public CollectionCardItem[] Import(ExcelTable table)
        {
            var items = new List<CollectionCardItem>();
            var allCards = _cardService.GetAll(true)
                .Where(it => it.VariantId > 0)
                .GroupBy(it => it.GetMultipleCardAttributeValue("SWU Id")!.First())
                .ToDictionary(it => it.Key, it => it.ToArray());

            var start = table.Address.Start;
            var end = table.Address.End;

            var setColumn = table.Columns.First(it => it.Name == "Set");
            var cardNumberColumn = table.Columns.First(it => it.Name == "CardNumber");
            var countColumn = table.Columns.First(it => it.Name == "Count");
            var isFoilColumn = table.Columns.First(it => it.Name == "IsFoil");

            var foilVariantTypes = _variants.Where(it => it.ChildOfBase).Select(it => it.Id).ToArray();

            for (var r = start.Row + 1; r <= end.Row; r++)
            {
                var item = new CollectionCardItem();

                var cardIdString = table.WorkSheet.Cells[r, cardNumberColumn.Position + 1].Value?.ToString();

                if (!int.TryParse(cardIdString, out var cardId) || !allCards.ContainsKey(cardIdString))
                {
                    throw new Exception($"Could not find card with ID: {cardIdString}");
                }

                var setCode = table.WorkSheet.Cells[r, setColumn.Position + 1].Value?.ToString();

                var cards = allCards[cardIdString].Where(it => it.SetName.Equals(setCode, StringComparison.InvariantCultureIgnoreCase)).ToArray();

                if (cards.Length == 0)
                {
                    throw new Exception($"Could not find card with ID: {cardIdString}");
                }

                var isFoilString = table.WorkSheet.Cells[r, isFoilColumn.Position + 1].Value?.ToString();

                if (!bool.TryParse(isFoilString, out var isFoil))
                {
                    throw new Exception("IsFoil should be TRUE or FALSE");
                }

                var card = cards.FirstOrDefault(it => isFoil ? it.VariantTypeId is null : foilVariantTypes.Contains(it.VariantTypeId ?? 0));
                if (card is null)
                {
                    throw new Exception($"Could not find card with ID: {cardIdString}");
                }

                var countString = table.WorkSheet.Cells[r, countColumn.Position + 1].Value?.ToString();
                if (!int.TryParse(countString, out var count))
                {
                    continue;
                }

                items.Add(new CollectionCardItem
                {
                    CardId = card.BaseId,
                    VariantId = card.VariantId,
                    Amount = count,
                });
            }
            
            return [.. items];
        }*/

        public CollectionCardItem[] Import(DetailedCsvModel[] records)
        {
            var items = new List<CollectionCardItem>();
            var allCards = _cardService.GetAll(true)
                .Where(it => it.VariantId > 0)
                .GroupBy(it => it.GetMultipleCardAttributeValue("SWU Id")!.First())
                .ToDictionary(it => it.Key, it => it.ToArray());

            var allSets = _cardService.GetAllSets().ToDictionary(it => it.SetCode!.ToLowerInvariant(), it => it.Id);

            var foilVariantTypes = _variants.Where(it => it.ChildOfBase || it.ChildOf.HasValue).Select(it => it.Id).ToArray();

            foreach (var record in records)
            {
                var item = new CollectionCardItem();

                if (!int.TryParse(record.CardNumber, out var cardId) || !allCards.ContainsKey(record.CardNumber))
                {
                    throw new Exception($"Could not find card with ID: {record.CardNumber}");
                }

                if (!allSets.TryGetValue(record.Set.ToLowerInvariant(), out var setId))
                {
                    throw new Exception($"No set exists with code: {record.Set}");
                }
                
                var cards = allCards[record.CardNumber].Where(it => it.SetId == setId).ToArray();

                if (cards.Length == 0)
                {
                    throw new Exception($"Could not find card with ID: {record.CardNumber}");
                }

                if (!bool.TryParse(record.IsFoil, out var isFoil))
                {
                    throw new Exception("IsFoil should be TRUE or FALSE");
                }

                Card? card;
                if (!isFoil)
                {
                    card = cards.FirstOrDefault(it => it.VariantTypeId is null) ?? cards.FirstOrDefault(it => !foilVariantTypes.Contains(it.VariantTypeId ?? 0));
                }
                else
                {
                    card = cards.FirstOrDefault(it => foilVariantTypes.Contains(it.VariantTypeId ?? 0));
                }

                if (card is null)
                {
                    throw new Exception($"Could not find card with ID: {record.CardNumber}");
                }

                if (!int.TryParse(record.Count, out var count))
                {
                    continue;
                }

                items.Add(new CollectionCardItem
                {
                    CardId = card.BaseId,
                    VariantId = card.VariantId,
                    Amount = count,
                });
            }

            return [.. items];
        }
    }
}
