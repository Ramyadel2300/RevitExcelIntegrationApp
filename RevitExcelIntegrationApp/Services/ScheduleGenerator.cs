using Autodesk.Revit.DB;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Linq;
using RevitExcelIntegrationApp.Enums;
using System;

namespace RevitExcelIntegrationApp.Services
{
    internal class ScheduleGenerator
    {
        // built in parameters / fields to add in schedule
        List<BuiltInParameter> BiParams = new List<BuiltInParameter> { BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM };
        private readonly UIDocument uidoc;
        private readonly Document doc;
        public ScheduleGenerator(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
        }
        public TransactionStatus GenerateCategorySchedule(BuiltInCategory category, string selectedParameter)
        {
            QuantityParameter selectedQuantityParameter = (QuantityParameter)Enum.Parse(typeof(QuantityParameter), selectedParameter);

            TransactionStatus status = new TransactionStatus();
            BuiltInParameter builtInParameter = default;
            switch (selectedQuantityParameter)
            {
                case QuantityParameter.Length:
                    builtInParameter = BuiltInParameter.CURVE_ELEM_LENGTH;
                    break;
                case QuantityParameter.Area:
                    builtInParameter = BuiltInParameter.HOST_AREA_COMPUTED;
                    break;
                case QuantityParameter.Volume:
                    builtInParameter = BuiltInParameter.HOST_VOLUME_COMPUTED;
                    break;
            }
            BiParams.Add(builtInParameter);
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Generating Schedule");
                ElementId categoryId = new ElementId(category);
                ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, categoryId);
                schedule.Name = "Schedule25" + category.ToString();
                IList<SchedulableField> schedylableFields = schedule.Definition.GetSchedulableFields().ToList();
                foreach (SchedulableField schedylableField in schedylableFields)
                {
                    if (CheckField(schedylableField))
                        schedule.Definition.AddField(schedylableField);
                }
                if (selectedQuantityParameter == QuantityParameter.Count)
                {
                    var SharedParameterCountField = schedylableFields.FirstOrDefault(x => x.FieldType == ScheduleFieldType.Count);
                    schedule.Definition.AddField(SharedParameterCountField);
                }
                var priceGUID = GetParameterID(category, "Price");
                var SharedParameterPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == priceGUID);
                var totalPriceGUID = GetParameterID(category, "Total Price");
                var SharedParametertotalPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == totalPriceGUID);
                schedule.Definition.AddField(SharedParameterPriceField);
                CalculateCategoryTotalPrice(category, schedule);
                schedule.Definition.AddField(SharedParametertotalPriceField);
                status = t.Commit();
                uidoc.ActiveView = schedule;
            }
            return status;
        }
        private bool CheckField(SchedulableField schedulableField)
        {
            foreach (BuiltInParameter bip in BiParams)
            {
                if (new ElementId(bip) == schedulableField.ParameterId)
                    return true;
            }
            return false;
        }
        public ElementId GetParameterID(BuiltInCategory categoryNam, string parameterName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(categoryNam);
            ElementId priceParameterID = default;
            foreach (Element element in collector)
            {
                var parameter = element.LookupParameter(parameterName);
                if (parameter != null)
                {
                    priceParameterID = parameter.Id;
                    break;
                }
            }
            return priceParameterID;
        }
        private void CalculateCategoryTotalPrice(BuiltInCategory category, ViewSchedule schedule)
        {
            using (SubTransaction st = new SubTransaction(doc))
            {
                st.Start();
                var scheduleTableValues = GetScheduleData(schedule);
                var elements = Utilities.GetAllStructuralGraphicalElements(doc, category).Where(x=>x.);
                int count = 0;
                foreach (var element in elements)
                {
                    List<string> currentRow = scheduleTableValues[count];
                    double priceValue = 0;
                    if (!string.IsNullOrEmpty(currentRow[2]))
                        priceValue = double.Parse(currentRow[2]);
                    double quantityValue = 0;
                    double scheduleTextValue = ExtractNumericValue(currentRow[1]);
                    if (scheduleTextValue != -1)
                        quantityValue = scheduleTextValue;
                    double totalcost = quantityValue * priceValue;
                    var totalPriceParameter = element.LookupParameter("Total Price");
                    if (!totalPriceParameter.IsReadOnly)
                    {
                        totalPriceParameter.Set(totalcost);
                    }
                    //count++;
                }
                st.Commit();
            }
        }
        static double ExtractNumericValue(string input)
        {
            string numericString = "";
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    numericString += c;
                }
                else if (numericString.Length > 0) // Break loop if a non-digit character is found after digits
                {
                    break;
                }
            }

            double result;
            if (double.TryParse(numericString, out result))
            {
                return result;
            }
            else
            {
                // Handle error if the parsing fails, return default value or throw exception
                // In this case, I'm returning -1 indicating failure
                return -1;
            }
        }
        public List<List<string>> GetScheduleData(ViewSchedule schedule)
        {
            TableData table = schedule.GetTableData();
            TableSectionData section = table.GetSectionData(SectionType.Body);
            int nRows = section.NumberOfRows;
            int nColumns = section.NumberOfColumns;

            List<List<string>> scheduleData = new List<List<string>>();
            if (nRows > 1)
            {
                for (int i = 0; i < nRows; i++)
                {
                    List<string> rowData = new List<string>();
                    for (int j = 0; j < nColumns; j++)
                    {
                        rowData.Add(schedule.GetCellText(SectionType.Body, i, j));
                    }
                    scheduleData.Add(rowData);
                }
                scheduleData.RemoveAt(0);
                scheduleData.RemoveAt(scheduleData.FindIndex(x => string.IsNullOrEmpty(x[0])));
                scheduleData.RemoveAt(scheduleData.FindIndex(x => string.IsNullOrEmpty(x[1])));
            }
            return scheduleData;
        }

        public static List<Dictionary<string, string>> DataMapping(List<string> keyData, List<List<string>> valueData)
        {
            List<Dictionary<string, string>> items = new List<Dictionary<string, string>>();

            string prompt = "Key/Value";
            prompt += Environment.NewLine;

            foreach (List<string> list in valueData)
            {
                for (int key = 0, value = 0; key < keyData.Count && value < list.Count; key++, value++)
                {
                    Dictionary<string, string> newItem = new Dictionary<string, string>();

                    string k = keyData[key];
                    string v = list[value];
                    newItem.Add(k, v);
                    items.Add(newItem);
                }
            }
            return items;
        }
    }
}

