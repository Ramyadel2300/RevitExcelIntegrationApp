using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitExcelIntegrationApp.Services
{
    internal class RevitCtegoriesHandler
    {
        private readonly Document doc;
        public IEnumerable<BuiltInCategory> DocumentCurrentCategories { get;}
        public RevitCtegoriesHandler(Document doc)
        {
            this.doc = doc;
            DocumentCurrentCategories = GetCategoriesForCurrentDocument();
        }
        private IEnumerable<BuiltInCategory> GetCategoriesForCurrentDocument()
        {
            FilteredElementCollector myElements = new FilteredElementCollector(doc).WhereElementIsElementType();
            IEnumerable<BuiltInCategory>  documentCategory = myElements.Where(x => x.Category != null)
                             .Select(x => x.Category)
                             .GroupBy(x => x.Name)
                             .Select(x => x.FirstOrDefault().BuiltInCategory); //Get BuiltInCategory for Categories in Document
            return documentCategory;
        }
        public ObservableCollection<BuiltInCategory> FilterCategories(string searchInput)
        {
            if (!string.IsNullOrWhiteSpace(searchInput))
            {
                List<BuiltInCategory> ComponentSearchResult = GetCategoriesForCurrentDocument().Where(c => c.ToString().ToLower()
                                                                                                                       .Contains(searchInput
                                                                                                                       .ToLower()))
                                                                                                                       .Select(c => c)
                                                                                                                       .Distinct()
                                                                                                                       .ToList();
                return new ObservableCollection<BuiltInCategory>(ComponentSearchResult);
            }
            return null;
        }

    }
}
