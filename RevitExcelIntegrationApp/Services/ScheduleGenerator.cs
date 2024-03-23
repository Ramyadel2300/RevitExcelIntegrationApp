using Autodesk.Revit.DB;
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
                case "Count":
                    //builtInParameter = BuiltInParameter.Number;
                    break;
                default:
                    builtInParameter = BuiltInParameter.VOLUME_NET;
                    break;
            }
            BiParams.Add(builtInParameter);
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Generating Schedule");

                ElementId categoryId = new ElementId(category);

                ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, categoryId);
                schedule.Name = "Schedule+";
                IList<SchedulableField> schedylableFields = schedule.Definition.GetSchedulableFields().ToList();
                var priceGUID = GetPriceID(category);
                var SharedParameterPriceField = schedule.Definition.GetSchedulableFields().FirstOrDefault(x => x.ParameterId == priceGUID);
                foreach (SchedulableField schedylableField in schedylableFields)
                {
                    if (CheckField(schedylableField))
                        schedule.Definition.AddField(schedylableField);
                }
                schedule.Definition.AddField(SharedParameterPriceField);
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
        public ElementId GetPriceID(BuiltInCategory categoryNam)
        {
            // Create a collector for elements of the specified category
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(categoryNam);
            ElementId priceParameterID = default;
            foreach (Element element in collector)
            {
                var parameters = element.Parameters;
                foreach (Parameter parameter in element.Parameters)
                {
                    if (parameter.Definition.Name == "Price")
                    {
                        priceParameterID = parameter.Id;
                        break;
                    }
                }
            }
            return priceParameterID;
        }
    }
}
