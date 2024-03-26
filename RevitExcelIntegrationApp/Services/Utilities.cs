using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RevitExcelIntegrationApp.Services
{
    internal class Utilities
    {
        public static List<Element> GetAllStructuralGraphicalElements(Document document, params BuiltInCategory[] builtInCategories)
        {
            ElementMulticategoryFilter elementFilters = new ElementMulticategoryFilter(builtInCategories);
            var structuralInstances = new FilteredElementCollector(document).WherePasses(elementFilters).WhereElementIsNotElementType().ToList();
            return structuralInstances;
        }
        public static bool LoadingSharedParamterFile(Document doc, BuiltInCategory newCategory)
        {
            string categoryGroupName = newCategory.ToString().Remove(0, 4) + "Cost Analysis Parameter";
            TransactionStatus transactionStatus;
            using (Transaction t = new Transaction(doc, $"Load Shared Parameter For {newCategory}"))
            {
                t.Start();
                var dllLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var dllParentFolder = Directory.GetParent(dllLocation);
                string sharedParamterFilePath = Path.Combine(dllParentFolder.FullName, "ELK Grove SharedParameters File.txt");
                SharedParameterProvider.AddSharedParameters(doc, newCategory, categoryGroupName, sharedParamterFilePath, "Price");
                SharedParameterProvider.AddSharedParameters(doc, newCategory, categoryGroupName, sharedParamterFilePath, "Total Price",false);
                transactionStatus = t.Commit();
            }
            if (transactionStatus == TransactionStatus.Committed)
                return true;
            else
                return false;
        }
    }
}
