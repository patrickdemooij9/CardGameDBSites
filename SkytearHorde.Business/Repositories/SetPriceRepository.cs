using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SkytearHorde.Business.Repositories
{
    public class SetPriceRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SetPriceRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public SetPriceRecordDBModel? GetLatestPrice(int setId)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database.FirstOrDefault<SetPriceRecordDBModel>(
                "SELECT * FROM SetPriceRecord WHERE SetId = @0 AND IsLatest = 1",
                setId);
        }

        public List<SetPriceRecordDBModel> GetPriceHistory(int setId)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database.Fetch<SetPriceRecordDBModel>(
                "SELECT * FROM SetPriceRecord WHERE SetId = @0 ORDER BY DateUtc ASC, Id ASC",
                setId);
        }

        public void InsertPrice(int setId, double totalPrice)
        {
            var today = DateTime.UtcNow.Date;

            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            var existingRecord = scope.Database.FirstOrDefault<SetPriceRecordDBModel>(
                "SELECT * FROM SetPriceRecord WHERE SetId = @0 AND IsLatest = 1",
                setId);

            if (existingRecord != null && existingRecord.DateUtc == today)
            {
                var predecessorPrice = existingRecord.TotalPrice - existingRecord.Delta;
                existingRecord.TotalPrice = totalPrice;
                existingRecord.Delta = totalPrice - predecessorPrice;
                scope.Database.Update(existingRecord);
                return;
            }

            if (existingRecord != null && existingRecord.TotalPrice == totalPrice)
            {
                return;
            }

            var newRecord = new SetPriceRecordDBModel
            {
                SetId = setId,
                TotalPrice = totalPrice,
                DateUtc = today,
                IsLatest = true,
                Delta = existingRecord != null ? totalPrice - existingRecord.TotalPrice : 0.0
            };
            scope.Database.Insert(newRecord);

            if (existingRecord != null)
            {
                existingRecord.IsLatest = false;
                scope.Database.Update(existingRecord);
            }
        }
    }
}
