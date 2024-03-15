using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace RevitExcelIntegrationApp
{
    public partial class RevitExcelUI : Window
    {
        public RevitExcelUI(UIDocument uidoc, Document doc)
        {
            InitializeComponent();
            this.DataContext = new AppMainVM(uidoc, doc);
        }
    }
}
