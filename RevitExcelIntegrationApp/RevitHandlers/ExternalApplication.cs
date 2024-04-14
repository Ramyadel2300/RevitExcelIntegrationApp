using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace RevitExcelIntegrationApp.RevitHandlers
{
    public class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("Blue Construction");
            string path = Assembly.GetExecutingAssembly().Location;
            RibbonPanel MyPanel = application.CreateRibbonPanel("Blue Construction", "Revit-Excel Pricing Integration");
            PushButtonData InsertPricesFromExcelButton = new PushButtonData("Button1", "RevLynx", path, "RevitExcelIntegrationApp.RevitHandlers.ExternalCommand");
            string outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string IconPath = Path.Combine(outPutDirectory, "Images\\ButtonIcon.png");
            InsertPricesFromExcelButton.LargeImage = new BitmapImage(new Uri(IconPath));
            InsertPricesFromExcelButton.ToolTip = "Allows To Insert Prices into Revit Elements";
            MyPanel.AddItem((RibbonItemData)(object)InsertPricesFromExcelButton);
            return 0;
        }
    }
}