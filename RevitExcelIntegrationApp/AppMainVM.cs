using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using RevitExcelIntegrationApp.Command;
using RevitExcelIntegrationApp.Services;

namespace RevitExcelIntegrationApp
{
    public class AppMainVM : BindableBase
    {
        private UIDocument uidoc;
        private Document doc;
        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get { return _selectedFilePath; }
            set { SetProperty(ref _selectedFilePath, value); }
        }

        public DelegateCommand LoadElementPriceFromExcelCommand {  get; set; }
        public DelegateCommand AddPricesToRevitElementsCommand {  get; set; }

        public AppMainVM(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
            SelectedFilePath = "Selected File Path...";
            LoadElementPriceFromExcelCommand = new DelegateCommand(ExecuteLoadElementPriceFromExcel);
            AddPricesToRevitElementsCommand = new DelegateCommand(ExecuteAddPricesToRevitElements);
        }

        private void ExecuteAddPricesToRevitElements(object obj)
        {
            if(SelectedFilePath is null)
                return;
            ExcelDataReader dataReader = new ExcelDataReader(SelectedFilePath);
            ElementPricesInserter pricesInserter = new ElementPricesInserter(doc, dataReader);
            pricesInserter.InsertPrices();
        }

        private void ExecuteLoadElementPriceFromExcel(object obj)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "xlsx Excel File (*.xlsx)|*.xlsx|xls Excel File (*.xls)|*.xls|All files (*.*)|*.*";
            dialog.ShowDialog();
            SelectedFilePath = dialog.FileName;
        }
    }
}