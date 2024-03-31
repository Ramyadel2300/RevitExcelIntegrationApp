using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RevitExcelIntegrationApp.Services
{
    public class RevitCategoriesHandler
    {
        private readonly Document doc;
        public IEnumerable<BuiltInCategory> DocumentCurrentCategories { get;}
        public RevitCategoriesHandler(Document doc)
        {
            this.doc = doc;
            DocumentCurrentCategories = GetCategoriesForCurrentDocument();
        }
        public IEnumerable<BuiltInCategory> GetCategoriesForCurrentDocument()
        {
            FilteredElementCollector myElements = new FilteredElementCollector(doc).WhereElementIsElementType();
            IEnumerable<BuiltInCategory>  documentCategory = myElements.Where(x => x.Category != null)
                             .Select(x => x.Category)
                             .GroupBy(x => x.Name)
                             .Select(x => x.FirstOrDefault().BuiltInCategory); //Get BuiltInCategory for Categories in Document
            return documentCategory;
        }

        public ObservableCollection<BuiltInCategory> GetCategoriesWithPriceSharedParameter(IEnumerable<BuiltInCategory> elementsCategories)
        {
            var sharedParametersFilePath = Utilities.GetSharedParameterFilePath(doc);
            ObservableCollection<BuiltInCategory> selectedCategories = new ObservableCollection<BuiltInCategory>();
            if (File.Exists(sharedParametersFilePath))
            {
                using (FileStream fileStream = File.Open(sharedParametersFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bufferedStream = new BufferedStream(fileStream))
                using (StreamReader reader = new StreamReader(bufferedStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("GROUP"))
                        {
                            var categoryPart = line.Split('\t')[2];
                            int constantPartIndex = categoryPart.IndexOf("Cost Analysis Parameter", StringComparison.OrdinalIgnoreCase);

                            if (constantPartIndex != -1)
                            {
                                string builtInCatgeoryName = categoryPart.Substring(0, constantPartIndex);
                                var builtInCategory = elementsCategories.FirstOrDefault(o => o.ToString() == builtInCatgeoryName);
                                if(!selectedCategories.Contains(builtInCategory))
                                    selectedCategories.Add(builtInCategory);
                            }
                        }
                    }
                }
            }
            return selectedCategories;

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
