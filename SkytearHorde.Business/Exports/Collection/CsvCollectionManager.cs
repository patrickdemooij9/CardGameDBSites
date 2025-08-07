using CsvHelper;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;
using System.Globalization;

namespace SkytearHorde.Business.Exports.Collection
{
    public class CsvCollectionManager : ICollectionExport
    {
        private readonly VariantTypeViewModel[] _variants;
        private readonly CardService _cardService;
        private readonly ImportMapping[] _importMappings;

        public CsvCollectionManager(VariantTypeViewModel[] variants, CardService cardService, ImportMapping[]  importMappings)
        {
            _variants = variants;
            _cardService = cardService;
            _importMappings = importMappings;
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

            return new DetailedCollectionImport(_variants, _cardService, _importMappings).Import(records);
        }
    }
}
