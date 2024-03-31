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
        public TransactionStatus GenerateCategorySchedule(BuiltInCategory category, string selectedParameter, string scheduleName)
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
                schedule.Name = scheduleName;
                IList<SchedulableField> schedylableFields = schedule.Definition.GetSchedulableFields().ToList();
                foreach (SchedulableField schedylableField in schedylableFields)
                {
                    if (CheckField(schedylableField))
                    {
                        ScheduleField scheduleField=schedule.Definition.AddField(schedylableField);
                        // schedule's group sorting (to collect family and type field)
                        if (schedylableField.ParameterId == new ElementId(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM))
                        {

                            // create group sorting (family and type)
                            FamilyTypeSorting = new ScheduleSortGroupField(scheduleField.FieldId);
                            // add schedule's group sorting field
                            schedule.Definition.AddSortGroupField(FamilyTypeSorting);
                        }
                    }
                }
                CalculateCategoryTotalPrice(category, selectedParameter);
                var priceGUID = GetParameterID(category, "Price");
                var SharedParameterPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == priceGUID);
                var totalPriceGUID = GetParameterID(category, "Total Price");
                var SharedParametertotalPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == totalPriceGUID);
                schedule.Definition.AddField(SharedParameterPriceField);
                schedule.Definition.AddField(SharedParametertotalPriceField);
                schedule.Definition.SetSortGroupField(0, FamilyTypeSorting);
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
            QuantityParameter selectedQuantityParameter = (QuantityParameter)Enum.Parse(typeof(QuantityParameter), schulableParameterName);

            using (SubTransaction st = new SubTransaction(doc))
            {
                st.Start();
                var elements = Utilities.GetAllStructuralGraphicalElements(doc, category);
                foreach (var element in elements)
                {
                    var totalPriceParameter = element.LookupParameter("Total Price");
                    var priceParameter = element.LookupParameter("Price").AsDouble();
                    Parameter schulableParameter = default;
                    switch (selectedQuantityParameter)
                    {
                        case QuantityParameter.Length:
                            schulableParameter = element.LookupParameter("Length");
                            break;
                        case QuantityParameter.Area:
                            schulableParameter = element.LookupParameter("Area");
                            break;
                        case QuantityParameter.Volume:
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

