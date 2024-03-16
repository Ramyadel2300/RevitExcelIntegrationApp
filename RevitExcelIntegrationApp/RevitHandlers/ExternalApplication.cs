using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace RevitExcelIntegrationApp.RevitHandlers
{
    public class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("ExcelIntegration");
            string path = Assembly.GetExecutingAssembly().Location;
            RibbonPanel MyPanel = application.CreateRibbonPanel("ExcelIntegration", "Revit-Excel Integration");
            PushButtonData InsertPricesFromExcelButton = new PushButtonData("Button1", "R-Excel", path, "RevitExcelIntegrationApp.RevitHandlers.ExternalCommand");
            string outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string IconPath = Path.Combine(outPutDirectory, "Images\\ButtonIcon.png");
            InsertPricesFromExcelButton.LargeImage = new BitmapImage(new Uri(IconPath));
            InsertPricesFromExcelButton.ToolTip = "Allows To Insert Prices into Revit Elements";
            MyPanel.AddItem((RibbonItemData)(object)InsertPricesFromExcelButton);
            return 0;
        }
    }
}