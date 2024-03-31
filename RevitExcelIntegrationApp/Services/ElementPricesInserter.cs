using Autodesk.Revit.DB;
using System;
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
        public bool InsertPrices()
        {
            TransactionStatus transactionStatus;
            var excelValues = elementPrices.ExcelReading();
            var categoiresPrices = excelValues.Select(v =>
            {
                BuiltInCategory category;
                Enum.TryParse("OST_" + v.Key.Trim(), out category);
                return new { category, v.Value };
            }).ToList();
            using (Transaction transaction = new Transaction(doc, "Inserting Price Into Elements"))
            {
                transaction.Start();
                for (int i = 0; i < categoiresPrices.Count; i++)
                {
                    var elements = Utilities.GetAllStructuralGraphicalElements(doc, categoiresPrices[i].category);
                    AddingPriceToElements(elements, categoiresPrices[i].Value);
                }
                transactionStatus = transaction.Commit();
            }
            if (transactionStatus == TransactionStatus.Committed)
                return true;
            else
                return false;
        }
        private void AddingPriceToElements(List<Element> elements, double price)
        {
            foreach (var element in elements)
            {
                Parameter parameter = element.LookupParameter(Constants.Price);
                if (parameter == null)
                    break;
                if (!parameter.IsReadOnly)
                {
                    parameter.Set(price);
                }
            }
        }
    }
}
