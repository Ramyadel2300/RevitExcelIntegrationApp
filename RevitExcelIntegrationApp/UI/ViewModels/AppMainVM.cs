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

namespace RevitExcelIntegrationApp.UI.ViewModels
{
    public class AppMainVM : BindableBase
    {
        private UIDocument uidoc;
        private Document doc;

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
            GetCategoriesWithPriceSharedParameter();
        }

        private void GetCategoriesWithPriceSharedParameter()
        {
            try
            {
                FilteredElementCollector myElements = new FilteredElementCollector(doc).WhereElementIsElementType();
                List<BuiltInCategory> categories = myElements.Where(x => x.Category != null)
                                                      .Select(x => x.Category)
                                                      .GroupBy(x => x.Name).Select(x=> x.FirstOrDefault().BuiltInCategory)
                                                      .ToList(); //Get BuiltInCategory for Categories in Document

                using (Transaction t = new Transaction(doc, $"Try to get Categories with Price Parameter"))
                {
                    TransactionStatus status = new TransactionStatus();
                    t.Start();
                    foreach (BuiltInCategory category in categories)
                    {
                        List<Element> instancesOfCategory = new FilteredElementCollector(doc)
                                                            .OfCategory(category).WhereElementIsNotElementType() //WhereElementIsNotElementType for instances parameetrs but WhereElementIsElementType for types 
                                                            .Cast<Element>()
                                                            .ToList(); //Get All Instances for each category
                        //bool categroyAdded = false;
                        //foreach (var instance in instancesOfCategory)
                        //{
                        //    foreach (Parameter parameetr in instance.Parameters)
                        //    {
                        //        if (parameetr?.Definition?.Name == "Price")
                        //        {
                        //            SelectedCategories.Add(category.ToString());
                        //            categroyAdded = true;
                        //            break;
                        //        }
                        //    }
                        //    if (categroyAdded) break;
                        //}
                        
                        if(instancesOfCategory.Any(i => i.Parameters.Cast<Parameter>().FirstOrDefault(p => p.Definition.Name == "Price") != null))
                            SelectedCategories.Add(category.ToString());
                    }
                    status = t.Commit();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
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

                    if (!SelectedCategories.Contains(SelectedCategory))
                        SelectedCategories.Add(SelectedCategory);
                }
            }
            catch (Exception ex)
            {
                PromptText = ex.Message;
            }
        }
        private void GenerateSchedule(object obj)
        {
            ScheduleGenerator scheduleGenerator = new ScheduleGenerator(uidoc, doc);
            scheduleGenerator.GenerateCategorySchedule(BuiltInCategory.OST_Walls, "Volume");
        }
        #endregion

        #region UI Pindeing Properties
        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get { return _selectedFilePath; }
            set { SetProperty(ref _selectedFilePath, value); }
        }
        ObservableCollection<BuiltInCategory> elementsCategories = new ObservableCollection<BuiltInCategory>(Enum.GetValues(typeof(BuiltInCategory)) as BuiltInCategory[]);
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
                    FilterCategories(value);
            }
        }
        private string prompt;

        public string PromptText
        {
            get { return prompt; }
            set { SetProperty(ref prompt, value); }
        }


        public ObservableCollection<string> SelectedCategories { get; set; } = new ObservableCollection<string>();

        private string selectedCategoryToSchedule;
        public string SelectedCategoryToSchedule
        {
            get { return selectedCategoryToSchedule; }
            set { SetProperty(ref selectedCategoryToSchedule, value); }
        }

        public ObservableCollection<string> QuantityParameters { get; set; } = new ObservableCollection<string>() { "Length", "Area", "Volume", "Count" };

        private string selectedQuantityParameter;
        public string SelectedQuantityParameter
        {
            get { return selectedQuantityParameter; }
            set { SetProperty(ref selectedQuantityParameter, value); }
        }

        #endregion

        private void FilterCategories(string searchInput)
        {
            ObservableCollection<BuiltInCategory> elementsCategories = new ObservableCollection<BuiltInCategory>(Enum.GetValues(typeof(BuiltInCategory)) as BuiltInCategory[]);
            //Making ListBoxItems to reposnd to the real time impact in the UI
            if (string.IsNullOrWhiteSpace(searchInput))
            {
                ElementsCategories = new ObservableCollection<BuiltInCategory>(elementsCategories.Distinct().ToList());
                return;
            }
            else
            {
                List<BuiltInCategory> ComponentSearchResult = elementsCategories.Where(c => c.ToString().ToLower()
                                                                                    .Contains(SearchInput
                                                                                    .ToLower())).Select(c => c)
                                                                                                .Distinct()
                                                                                                .ToList();
                ElementsCategories = new ObservableCollection<BuiltInCategory>(ComponentSearchResult);
            }
        }
    }
}