using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitExcelIntegrationApp.Command;
using System.ComponentModel;

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

        }

        private void ExecuteLoadElementPriceFromExcel(object obj)
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}