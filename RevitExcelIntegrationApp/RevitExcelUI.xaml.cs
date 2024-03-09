using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows;

namespace RevitExcelIntegrationApp
{
    /// <summary>
    /// Interaction logic for RevitExcelUI.xaml
    /// </summary>
    public partial class RevitExcelUI : Window
    {
        public RevitExcelUI(UIDocument uidoc, Document doc)
        {
            InitializeComponent();
            this.DataContext = new AppMainVM(uidoc, doc);
        }
    }
}
