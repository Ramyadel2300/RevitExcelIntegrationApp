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
        public static void LoadingSharedParamterFile(Document doc)
        {
            using (SubTransaction t = new SubTransaction(doc))
            {
                t.Start();
                var dllLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var dllParentFolder = Directory.GetParent(dllLocation);
                string sharedParamterFilePath = Path.Combine(dllParentFolder.FullName, "ELK Grove SharedParameters File.txt");
                SharedParameterProvider.AddSharedParameters(doc, BuiltInCategory.OST_StructuralColumns, "Column Cost Analysis Parameter", sharedParamterFilePath, "Price");
                SharedParameterProvider.AddSharedParameters(doc, BuiltInCategory.OST_StructuralFraming, "Framing Cost Analysis Parameter", sharedParamterFilePath, "Price");
                t.Commit();
            }
        }
    }
}
