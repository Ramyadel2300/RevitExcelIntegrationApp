﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using RevitExcelIntegrationApp.Command;
using RevitExcelIntegrationApp.Services;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using RevitExcelIntegrationApp.UI.Services;

namespace RevitExcelIntegrationApp.UI.ViewModels
{
    public class AppMainVM : BindableBase
    {
        private UIDocument uidoc;
        private Document doc;
        public event EventHandler CloseRequested;

        public DelegateCommand LoadElementPriceFromExcelCommand { get; set; }
        public DelegateCommand AddPricesToRevitElementsCommand { get; set; }
        public DelegateCommand AddSharedParameterCommand { get; set; }

        public AppMainVM(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
            SelectedFilePath = "Selected File Path...";
            LoadElementPriceFromExcelCommand = new DelegateCommand(ExecuteLoadElementPriceFromExcel);
            AddPricesToRevitElementsCommand = new DelegateCommand(ExecuteAddPricesToRevitElements);
            AddSharedParameterCommand = new DelegateCommand(LoadSharedParameter);
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
                    PromptText = "Excel Prices Are Inserted Successfully";
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
                PromptText = "Excel Prices Are Loaded Successfully";
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
                    PromptText = $"Price is Added To  {selected} Successfully";
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