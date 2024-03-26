﻿using Autodesk.Revit.DB;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Linq;

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
            TransactionStatus status = new TransactionStatus();
            BuiltInParameter builtInParameter = default;
            switch (selectedParameter)
            {
                case "Length":
                    builtInParameter = BuiltInParameter.CURVE_ELEM_LENGTH;
                    break;
                case "Area":
                    builtInParameter = BuiltInParameter.HOST_AREA_COMPUTED;
                    break;
                case "Volume":
                    builtInParameter = BuiltInParameter.HOST_VOLUME_COMPUTED;
                    break;
            }
            BiParams.Add(builtInParameter);
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Generating Schedule");
                ElementId categoryId = new ElementId(category);
                ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, categoryId);
                schedule.Name = "Schedule1";
                IList<SchedulableField> schedylableFields = schedule.Definition.GetSchedulableFields().ToList();
                foreach (SchedulableField schedylableField in schedylableFields)
                {
                    if (CheckField(schedylableField))
                        schedule.Definition.AddField(schedylableField);
                }
                CalculateCategoryTotalPrice(category, selectedParameter);
                var priceGUID = GetParameterID(category, "Price");
                var SharedParameterPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == priceGUID);
                var totalPriceGUID = GetParameterID(category, "Total Price");
                var SharedParametertotalPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == totalPriceGUID);
                schedule.Definition.AddField(SharedParameterPriceField);
                schedule.Definition.AddField(SharedParametertotalPriceField);
                if (selectedParameter == "Count")
                {
                    var SharedParameterCountField = schedylableFields.FirstOrDefault(x => x.FieldType == ScheduleFieldType.Count);
                    schedule.Definition.AddField(SharedParameterCountField);
                }
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
        private void CalculateCategoryTotalPrice(BuiltInCategory category, string schulableParameterName)
        {
            using (SubTransaction st = new SubTransaction(doc))
            {
                st.Start();
                var elements = Utilities.GetAllStructuralGraphicalElements(doc, category);
                foreach (var element in elements)
                {
                    var totalPriceParameter = element.LookupParameter("Total Price");
                    var priceParameter = element.LookupParameter("Price").AsDouble();
                    Parameter schulableParameter = default;
                    switch (schulableParameterName)
                    {
                        case "Length":
                            schulableParameter = element.LookupParameter("Length");
                            break;
                        case "Area":
                            schulableParameter = element.LookupParameter("Area");
                            break;
                        case "Volume":
                            schulableParameter = element.LookupParameter("Volume");
                            break;
                    }
                    if (!totalPriceParameter.IsReadOnly && schulableParameter != null)
                    {
                        double totalcost = priceParameter * schulableParameter.AsDouble();
                        totalPriceParameter.Set(totalcost);
                    }
                }
                st.Commit();
            }
        }
    }
}

