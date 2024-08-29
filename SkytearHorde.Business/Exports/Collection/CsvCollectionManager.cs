using CsvHelper;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;
using System.Globalization;

namespace SkytearHorde.Business.Exports.Collection
{
    public class CsvCollectionManager : ICollectionExport
    {
        private readonly VariantTypeViewModel[] _variants;
        private readonly CardService _cardService;

        public CsvCollectionManager(VariantTypeViewModel[] variants, CardService cardService)
        {
            _variants = variants;
            _cardService = cardService;
        }

        public Task<byte[]> Export(CollectionCardItem[] collection)
        {
            throw new NotImplementedException();
        }

        public CollectionCardItem[] Import(Stream data)
        {
            using var reader = new StreamReader(data);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<DetailedCsvModel>().ToArray();

            return new DetailedCollectionImport(_variants, _cardService).Import(records);
        }
    }
}
