using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using RevitExcelIntegrationApp.Command;
using RevitExcelIntegrationApp.Services;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using RevitExcelIntegrationApp.UI.Services;
using System.Windows;
using RevitExcelIntegrationApp.Enums;
using System.IO;

namespace RevitExcelIntegrationApp.UI.ViewModels
{
    public class AppMainVM : BindableBase
    {
        private UIDocument uidoc;
        private Document doc;
        RevitCtegoriesHandler ctegoriesHandler;
        public DelegateCommand LoadElementPriceFromExcelCommand { get; set; }
        public DelegateCommand AddPricesToRevitElementsCommand { get; set; }
        public DelegateCommand AddSharedParameterCommand { get; set; }
        public DelegateCommand GenerateScheduleCommand { get; set; }
        public AppMainVM(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
            SelectedFilePath = "Selected File Path...";
            LoadElementPriceFromExcelCommand = new DelegateCommand(ExecuteLoadElementPriceFromExcel);
            AddPricesToRevitElementsCommand = new DelegateCommand(ExecuteAddPricesToRevitElements);
            AddSharedParameterCommand = new DelegateCommand(LoadSharedParameter);
            GenerateScheduleCommand = new DelegateCommand(GenerateSchedule);

            ctegoriesHandler = new RevitCtegoriesHandler(doc);
            elementsCategories = new ObservableCollection<BuiltInCategory>(GetCategoriesForCurrentDocument());
            GetCategoriesWithPriceSharedParameter();
        }

        private IEnumerable<BuiltInCategory> GetCategoriesForCurrentDocument()
        {
            FilteredElementCollector myElements = new FilteredElementCollector(doc).WhereElementIsElementType();
            return myElements.Where(x => x.Category != null)
                                                  .Select(x => x.Category)
                                                  .GroupBy(x => x.Name).Select(x => x.FirstOrDefault().BuiltInCategory); //Get BuiltInCategory for Categories in Document
        }
        private void GetCategoriesWithPriceSharedParameter()
        {
            var sharedParametersFilePath = Utilities.GetSharedParameterFilePath(doc);
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
                                if (!SelectedCategories.Contains(builtInCategory))
                                    SelectedCategories.Add(builtInCategory);

                            }
                        }
                    }
                }
            }

        }

        #region UI Commands
        private void ExecuteAddPricesToRevitElements(object obj)
        {
            try
            {
                if (SelectedFilePath is null)
                    return;
                ExcelDataReader dataReader = new ExcelDataReader(SelectedFilePath);
                ElementPricesInserter pricesInserter = new ElementPricesInserter(doc, dataReader);
                var areInserted = pricesInserter.InsertPrices();
                if (areInserted)
                    PromptText = "Excel Prices are Inserted Successfully";
            }
            catch (Exception ex)
            {
                PromptText = ex.Message;
            }
        }
        private void ExecuteLoadElementPriceFromExcel(object obj)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "xlsx Excel File (*.xlsx)|*.xlsx|xls Excel File (*.xls)|*.xls|All files (*.*)|*.*";
                dialog.ShowDialog();
                SelectedFilePath = dialog.FileName;
                if (!string.IsNullOrEmpty(SelectedFilePath))
                    PromptText = "Excel Prices are Loaded Successfully";
                else
                    PromptText = "No File is Selected";
            }
            catch (Exception ex)
            {
                PromptText = ex.Message;
            }
        }
        private void LoadSharedParameter(object obj)
        {
            try
            {
                var selected = elementsCategories.Where(o => o.ToString() == SelectedCategory).FirstOrDefault();
                var isLoaded = Utilities.LoadingSharedParamterFile(doc, selected);
                if (isLoaded)
                {
                    PromptText = $"Price is Added to {selected} Successfully";

                    if (!SelectedCategories.Contains(selected))
                        SelectedCategories.Add(selected);
                }
            }
            catch (Exception ex)
            {
                PromptText = ex.Message;
            }
        }
        private void GenerateSchedule(object obj)
        {
            try
            {
                ScheduleGenerator scheduleGenerator = new ScheduleGenerator(uidoc, doc);
                var selected = QuantityParameters.Where(o => o.ToString() == SelectedQuantityParameter).FirstOrDefault();
                scheduleGenerator.GenerateCategorySchedule(BuiltInCategory.OST_Walls, selected.ToString());
            }
            catch (Exception ex)
            {
                PromptText = ex.Message;
            }
        }
        #endregion

        #region UI Pindeing Properties
        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get { return _selectedFilePath; }
            set { SetProperty(ref _selectedFilePath, value); }
        }
        ObservableCollection<BuiltInCategory> elementsCategories;
        public ObservableCollection<BuiltInCategory> ElementsCategories
        {
            get { return elementsCategories; }
            set { SetProperty(ref elementsCategories, value); }

        }
        private string selectedCategory;
        public string SelectedCategory
        {
            get { return selectedCategory; }
            set { SetProperty(ref selectedCategory, value); }

        }
        private string searchInput;

        public string SearchInput
        {
            get { return searchInput; }
            set
            {
                if (SetProperty(ref searchInput, value))
                    ElementsCategories = ctegoriesHandler.FilterCategories(value);
            }
        }
        private string prompt;

        public string PromptText
        {
            get { return prompt; }
            set { SetProperty(ref prompt, value); }
        }


        public ObservableCollection<BuiltInCategory> SelectedCategories { get; set; } = new ObservableCollection<BuiltInCategory>();

        private string selectedCategoryToSchedule;
        public string SelectedCategoryToSchedule
        {
            get { return selectedCategoryToSchedule; }
            set { SetProperty(ref selectedCategoryToSchedule, value); }
        }

        public ObservableCollection<QuantityParameter> QuantityParameters { get; set; } = new ObservableCollection<QuantityParameter>(Enum.GetValues(typeof(QuantityParameter)) as QuantityParameter[]);

        private string selectedQuantityParameter;
        public string SelectedQuantityParameter
        {
            get { return selectedQuantityParameter; }
            set { SetProperty(ref selectedQuantityParameter, value); }
        }

        #endregion

    }
}