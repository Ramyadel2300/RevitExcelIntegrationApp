using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.ComponentModel;

namespace RevitExcelIntegrationApp
{
    internal class AppMainVM:INotifyPropertyChanged
    {
        private UIDocument uidoc;
        private Document doc;

        public AppMainVM(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}