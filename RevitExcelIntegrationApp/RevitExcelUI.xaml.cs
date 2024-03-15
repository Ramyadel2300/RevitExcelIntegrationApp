using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System.Data.OleDb;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataTable = System.Data.DataTable;

namespace RevitExcelIntegrationApp
{
    /// <summary>
    /// Interaction logic for RevitExcelUI.xaml
    /// </summary>
    public partial class RevitExcelUI : System.Windows.Window
    {
        private readonly UIDocument uidoc;
        private readonly Document doc;

        public RevitExcelUI(UIDocument uidoc, Document doc)
        {
            InitializeComponent();
            this.DataContext = new AppMainVM(uidoc, doc);
            this.uidoc = uidoc;
            this.doc = doc;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var excelValues=ExcelReading();
            using (Transaction transaction = new Transaction(doc, "CreatingSahredParamterFile"))
            {
                transaction.Start();
                var dllLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var dllParentFolder = Directory.GetParent(dllLocation);
                string sharedParamterFilePath = Path.Combine(dllParentFolder.FullName, "ELK Grove SharedParameters File.txt");
                SharedParameterProvider.AddSharedParameters(doc, BuiltInCategory.OST_StructuralColumns, "Cost Analysis Parameter", sharedParamterFilePath, "Column Price");
                SharedParameterProvider.AddSharedParameters(doc, BuiltInCategory.OST_StructuralFraming, "Cost Analysis Parameter", sharedParamterFilePath, "Framing Price");
                var structuralColumns=GetAllStructuralGraphicalElements(doc, BuiltInCategory.OST_StructuralColumns);
                var structuralFraming= GetAllStructuralGraphicalElements(doc, BuiltInCategory.OST_StructuralFraming);
                transaction.Commit();
            }
        }

        private Dictionary<string, double> ExcelReading()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "xlsx Excel File (*.xlsx)|*.xlsx|xls Excel File (*.xls)|*.xls|All files (*.*)|*.*";
            dialog.ShowDialog();
            string fileName = dialog.FileName;
            var datatable = ReadExcel(fileName, "xlsx");
            var totalSheetRows = datatable.Rows.Count;
            Dictionary<string, double> SheetRows = new Dictionary<string, double>();
            var dataTableRows = datatable.Rows;
            for (int i = 1; i < totalSheetRows; i++)//To Skip Header
            {
                SheetRows.Add((string)dataTableRows[i].ItemArray[0], (double)dataTableRows[i].ItemArray[1]);
            }
            return SheetRows;
        }
        public DataTable ReadExcel(string fileName, string fileExt)
        {
            string conn = string.Empty;
            DataTable dtexcel = new DataTable();
            if (fileExt.CompareTo(".xls") == 0) conn = @"provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties='Excel 8.0;HRD=Yes;IMEX=1';"; //for below excel 2007
            else conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=NO';"; //for above excel 2007
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1
                    oleAdpt.Fill(dtexcel); //fill excel data into dataTable
                }
                catch { }
            }
            return dtexcel;
        }
        public static List<Element> GetAllStructuralGraphicalElements(Document document, params BuiltInCategory[] builtInCategories)
        {
            ElementMulticategoryFilter elementFilters = new ElementMulticategoryFilter(builtInCategories);
            var structuralInstances = new FilteredElementCollector(document).WherePasses(elementFilters).WhereElementIsNotElementType().ToList();
            return structuralInstances;
        }
    }
}
