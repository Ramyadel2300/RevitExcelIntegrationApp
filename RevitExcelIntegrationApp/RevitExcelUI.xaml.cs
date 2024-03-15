using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System.Windows;
using RevitExcelIntegrationApp.Services;

namespace RevitExcelIntegrationApp
{
    /// <summary>
    /// Interaction logic for RevitExcelUI.xaml
    /// </summary>
    public partial class RevitExcelUI : Window
    {
        private readonly UIDocument uidoc;
        private readonly Document doc;

        public RevitExcelUI(UIDocument uidoc, Document doc)
        {
            InitializeComponent();
            this.uidoc = uidoc;
            this.doc = doc;
            this.DataContext = new AppMainVM(uidoc, doc);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "xlsx Excel File (*.xlsx)|*.xlsx|xls Excel File (*.xls)|*.xls|All files (*.*)|*.*";
            dialog.ShowDialog();
            string fileName = dialog.FileName;
            ExcelDataReader dataReader = new ExcelDataReader(fileName);
            ElementPricesInserter pricesInserter = new ElementPricesInserter(doc, dataReader);
            pricesInserter.InsertPrices();
        }
    }
}
