using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace RevitExcelIntegrationApp.Services
{
    internal class ElementPricesInserter
    {
        private readonly Document doc;
        private readonly ExcelDataReader elementPrices;

        public ElementPricesInserter(Document doc, ExcelDataReader elementPrices)
        {
            this.doc = doc;
            this.elementPrices = elementPrices;
        }
        public void InsertPrices()
        {
            var excelValues = elementPrices.ExcelReading();
            var columnPrice = excelValues.FirstOrDefault(v => v.Key.ToLower().Contains("column")).Value;
            var framingPrice = excelValues.FirstOrDefault(v => v.Key.ToLower().Contains("framing")).Value;
            var wallPrice = excelValues.FirstOrDefault(v => v.Key.ToLower().Contains("walls")).Value;
            using (Transaction transaction = new Transaction(doc, "Inserting Price Into Elements"))
            {
                transaction.Start();
                Utilities.LoadingSharedParamterFile(doc);
                var structuralColumns = Utilities.GetAllStructuralGraphicalElements(doc, BuiltInCategory.OST_StructuralColumns);
                AddingPriceToElements(structuralColumns, columnPrice);
                var structuralFraming = Utilities.GetAllStructuralGraphicalElements(doc, BuiltInCategory.OST_StructuralFraming);
                AddingPriceToElements(structuralFraming, framingPrice);
                transaction.Commit();
            }
        }

        private void AddingPriceToElements(List<Element> elements, double price)
        {
            foreach (var element in elements)
            {
                Parameter parameter = element.LookupParameter("Price");
                if (!parameter.IsReadOnly)
                {
                    parameter.Set(price.ToString());
                }
            }
        }
    }
}
