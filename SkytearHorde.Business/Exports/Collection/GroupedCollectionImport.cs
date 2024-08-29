using Microsoft.VisualBasic;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;
using System.IO;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Exports.Collection
{
    public class GroupedCollectionImport : IExcelCollectionImport
    {
        private readonly VariantTypeViewModel[] _variantTypes;
        private readonly Set[] _sets;
        private readonly CardService _cardService;

        public GroupedCollectionImport(VariantTypeViewModel[] variantTypes, Set[] sets, CardService cardService)
        {
            _variantTypes = variantTypes;
            _sets = sets;
            _cardService = cardService;
        }

        public bool CanImport(ExcelTable table)
        {
            return true; //Always try to import with this one as it has additional validation/error messages
        }

        public CollectionCardItem[] Import(ExcelTable table)
        {
            var items = new List<CollectionCardItem>();

            var setsDict = _sets.ToDictionary(it => it.Id, it => it);
            var allCards = new Dictionary<string, Card>();

            foreach (var set in _sets)
            {
                foreach (var card in _cardService.GetAllBaseBySet(set.Id))
                {
                    var cardId = card.GetMultipleCardAttributeValue("SWU Id")?.FirstOrDefault();
                    if (cardId is null) continue;

                    var identifier = $"{set.SetCode}{cardId}";
                    if (allCards.ContainsKey(identifier))
                    {
                        continue;
                    }

                    allCards.Add(identifier, card);
                }
            }

            var start = table.Address.Start;
            var end = table.Address.End;

            var variantsColumns = new Dictionary<int, int>();

            var setColumn = table.Columns.FirstOrDefault(it => it.Name.Equals("Set")) ?? throw new Exception("Could not find column with name 'Set'");
            var baseCardIdColumn = table.Columns.FirstOrDefault(it => it.Name.Equals("Base card id")) ?? throw new Exception("Could not find column with name 'Base card id'");

            var normalAmountColumn = table.Columns.FirstOrDefault(it => it.Name.Equals("Normal")) ?? throw new Exception("Could not find column with name 'Normal' for the amount");
            foreach (var variantType in _variantTypes)
            {
                //TODO: Fix this for reprints!!!
                var foundColumn = table.Columns.FirstOrDefault(it => it.Name.Equals(variantType.DisplayName));
                if (foundColumn != null)
                {
                    variantsColumns.Add(variantType.Id, foundColumn.Position);
                }
            }

            for (var r = start.Row + 1; r <= end.Row; r++)
            {
                var item = new CollectionCardItem();

                var setCode = table.WorkSheet.Cells[r, setColumn.Position + 1].Value?.ToString();
                var baseCardId = table.WorkSheet.Cells[r, baseCardIdColumn.Position + 1].Value?.ToString();
                var identifier = $"{setCode}{baseCardId}";
                if (string.IsNullOrWhiteSpace(identifier) || !allCards.ContainsKey(identifier)) continue;

                var card = allCards[identifier];
                var cardVariants = _cardService.GetVariantsForVariant(card.VariantId);
                if (normalAmountColumn != null)
                {
                    var normalAmount = table.WorkSheet.Cells[r, normalAmountColumn.Position + 1].GetValue<int?>();
                    if (normalAmount != null)
                    {
                        items.Add(new CollectionCardItem
                        {
                            CardId = card.BaseId,
                            VariantId = card.VariantId,
                            Amount = normalAmount.Value
                        });
                    }
                }
                foreach (var variant in variantsColumns)
                {
                    var variantValue = table.WorkSheet.Cells[r, variant.Value + 1].GetValue<int?>();
                    if (variantValue is null) continue;

                    var variantId = cardVariants.FirstOrDefault(it => it.VariantTypeId == variant.Key)?.VariantId;
                    if (variantId is null) continue;

                    items.Add(new CollectionCardItem
                    {
                        CardId = card.BaseId,
                        Amount = variantValue.Value,
                        VariantId = variantId.Value
                    });
                }
            }
            return items.ToArray();
        }
    }
}
