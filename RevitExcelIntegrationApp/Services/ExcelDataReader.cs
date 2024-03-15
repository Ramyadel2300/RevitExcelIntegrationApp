using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitExcelIntegrationApp.Services
{
    internal class ExcelDataReader
    {
        private readonly string fileName;

        public ExcelDataReader(string fileName)
        {
            this.fileName = fileName;
        }
        public Dictionary<string, double> ExcelReading()
        {
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
        private DataTable ReadExcel(string fileName, string fileExt)
        {
            string conn = string.Empty;
            DataTable dtexcel = new DataTable();
            if (fileExt.CompareTo(".xls") == 0) conn = @"provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties='Excel 8.0;HRD=Yes;IMEX=1';"; //for below excel 2007
            else conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=NO';"; //for above excel 2007
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1
                oleAdpt.Fill(dtexcel); //fill excel data into dataTable
            }
            return dtexcel;
        }
    }
}
