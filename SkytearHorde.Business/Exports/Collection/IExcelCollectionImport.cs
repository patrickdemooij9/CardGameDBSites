using OfficeOpenXml.Table;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Exports.Collection
{
    public interface IExcelCollectionImport
    {
        CollectionCardItem[] Import(ExcelTable table);

        bool CanImport(ExcelTable table);
    }
}
