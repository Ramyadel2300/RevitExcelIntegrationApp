﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using RevitExcelIntegrationApp.Command;
using RevitExcelIntegrationApp.Services;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using RevitExcelIntegrationApp.UI.Services;
using RevitExcelIntegrationApp.Enums;

namespace RevitExcelIntegrationApp.UI.ViewModels
{
    public class AppMainVM : BindableBase
    {
        private UIDocument uidoc;
        private Document doc;
        public RevitCategoriesHandler categoriesHandler { get; set; }
        public DelegateCommand LoadElementPriceFromExcelCommand { get; set; }
        public DelegateCommand AddPricesToRevitElementsCommand { get; set; }
        public DelegateCommand AddSharedParameterCommand { get; set; }
        public DelegateCommand GenerateScheduleCommand { get; set; }
        public AppMainVM(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
            LoadElementPriceFromExcelCommand = new DelegateCommand(ExecuteLoadElementPriceFromExcel);
            AddPricesToRevitElementsCommand = new DelegateCommand(ExecuteAddPricesToRevitElements);
            AddSharedParameterCommand = new DelegateCommand(LoadSharedParameter);
            GenerateScheduleCommand = new DelegateCommand(GenerateSchedule);

            categoriesHandler = new RevitCategoriesHandler(doc);
            elementsCategories = new ObservableCollection<BuiltInCategory>(categoriesHandler.GetCategoriesForCurrentDocument());
            SelectedCategories = categoriesHandler.GetCategoriesWithPriceSharedParameter(elementsCategories);
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
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                PromptText = "Failed Operation: Save this new Revit Model First.";
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
                {
                    PromptText = "No File is Selected";
                    SelectedFilePath= "Selected File Path...";
                }
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
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                PromptText = "Failed Operation: Save this new Revit Model First.";
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
                var selectedParameter = QuantityParameters.Where(o => o.ToString() == SelectedQuantityParameter).FirstOrDefault();
                var selectedCategoryToSchedule = SelectedCategories.Where(o => o.ToString() == SelectedCategoryToSchedule).FirstOrDefault();
                if (string.IsNullOrEmpty(scheduleName))
                    throw new Exception("Please, enter value for schedule name!");
                bool isScheduleGenertaed=scheduleGenerator.GenerateCategorySchedule(selectedCategoryToSchedule, selectedParameter.ToString(), scheduleName);
                if (isScheduleGenertaed)
                    PromptText = $"A {selectedParameter.ToString()} schedule is generated Successfully to selected category {selectedCategoryToSchedule.ToString()}";
                }
            catch (Autodesk.Revit.Exceptions.ArgumentException) 
            {
                PromptText = "schedule name allready exist, please change this name.";
            }
            catch (Exception ex)
            {
                PromptText = ex.Message;
            }
        }
        #endregion

        #region UI Pindeing Properties
        private string _selectedFilePath = "Selected File Path...";
        public string SelectedFilePath
        {
            get { return _selectedFilePath; }
            set { SetProperty(ref _selectedFilePath, value); }
        }
        ObservableCollection<BuiltInCategory> elementsCategories;
        public ObservableCollection<BuiltInCategory> ElementsCategories
        {
            get 
            {
                if(string.IsNullOrEmpty(SearchInput))
                    elementsCategories = new ObservableCollection<BuiltInCategory>(categoriesHandler.GetCategoriesForCurrentDocument());
                return elementsCategories;
            }
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
                    ElementsCategories = categoriesHandler.FilterCategories(value);
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
        
        private string scheduleName;
        public string ScheduleName
        {
            get { return scheduleName; }
            set { SetProperty(ref scheduleName, value); }
        }

        #endregion

    }
}