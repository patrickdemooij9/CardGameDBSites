using Microsoft.VisualBasic;
using OfficeOpenXml;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;

namespace SkytearHorde.Business.Exports.Collection
{
    public class ExcelCollectionManager : ICollectionExport
    {
        private readonly VariantTypeViewModel[] _variantTypes;
        private readonly Set[] _sets;
        private readonly CardService _cardService;

        public ExcelCollectionManager(VariantTypeViewModel[] variantTypes, Set[] sets, CardService cardService)
        {
            _variantTypes = variantTypes;
            _sets = sets;
            _cardService = cardService;
        }

        public Task<byte[]> Export(CollectionCardItem[] collection)
        {
            using var excelPackage = new ExcelPackage();
            var worksheet = excelPackage.Workbook.Worksheets.Add("Data");

            worksheet.Cells[1, 1].Value = "Set";
            worksheet.Cells[1, 2].Value = "Base card id";
            worksheet.Cells[1, 3].Value = "Name";
            worksheet.Cells[1, 4].Value = "Normal";

            var index = 5;
            foreach (var variant in _variantTypes)
            {
                worksheet.Cells[1, index].Value = variant.DisplayName;
                index++;
            }

            var row = 2;
            var collectionItems = collection.GroupBy(it => it.CardId).ToDictionary(it => it.Key, it => it);
            var setsDict = _sets.ToDictionary(it => it.Id, it => it);

            foreach (var set in _sets)
            {
                var setCards = _cardService.GetAllBySet(set.Id, true);
                var baseCards = setCards.Where(it => it.VariantTypeId is null && it.VariantId > 0).ToArray();
                var otherCards = setCards.Where(it => it.VariantTypeId.HasValue && it.VariantId > 0).ToDictionary(it => it.VariantId, it => it);

                foreach (var card in baseCards.OrderBy(it => it.GetMultipleCardAttributeValue("SWU Id")?.FirstOrDefault()))
                {
                    collectionItems.TryGetValue(card.BaseId, out var items);

                    var baseAmount = items?.FirstOrDefault(it => it.VariantId == card.VariantId)?.Amount ?? 0;

                    worksheet.Cells[row, 1].Value = set.SetCode;
                    worksheet.Cells[row, 2].Value = card.GetMultipleCardAttributeValue("SWU Id")?.FirstOrDefault();
                    worksheet.Cells[row, 3].Value = card.DisplayName;

                    if (baseAmount > 0)
                    {
                        worksheet.Cells[row, 4].Value = baseAmount;
                    }

                    var variantCards = items?.Where(it => otherCards.ContainsKey(it.VariantId)).ToDictionary(it => otherCards[it.VariantId].VariantTypeId!.Value, it => it) ?? [];
                    var variantIndex = 5;
                    foreach (var variant in _variantTypes)
                    {
                        var variantAmount = 0;
                        if (variantCards.TryGetValue(variant.Id, out var variantCard))
                        {
                            variantAmount = variantCard.Amount;
                        }

                        if (variantAmount > 0)
                        {
                            worksheet.Cells[row, variantIndex].Value = variantAmount;
                        }

                        variantIndex++;
                    }
                    row++;
                }
            }

            worksheet.Tables.Add(new ExcelAddressBase(1, 1, row - 1, index - 1), "Collection");

            return Task.FromResult(excelPackage.GetAsByteArray());
        }

        public CollectionCardItem[] Import(Stream data)
        {
            var importers = new IExcelCollectionImport[]
            {
                new DetailedExcelCollectionImport(_variantTypes, _cardService),
                new GroupedCollectionImport(_variantTypes, _sets, _cardService)
            };

            using var package = new ExcelPackage(data);
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Tables.Count < 1)
            {
                throw new Exception("Could not find a table to import. Make sure to create a table in your data.");
            }
            var table = worksheet.Tables[0];

            var importerToUse = importers.First(it => it.CanImport(table));
            return importerToUse.Import(table);
        }
    }
}
