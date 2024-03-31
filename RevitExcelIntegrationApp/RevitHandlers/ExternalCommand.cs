using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitExcelIntegrationApp.RevitHandlers
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class ExternalCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;
                RevitExcelUI Window = new RevitExcelUI(uidoc, doc);
                Window.ShowDialog();
                doc.Save();//saving all perfromed changes
                return 0;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
