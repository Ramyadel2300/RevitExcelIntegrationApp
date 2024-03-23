using Autodesk.Revit.DB;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Linq;

namespace RevitExcelIntegrationApp.Services
{
    internal class ScheduleGenerator
    {
        // built in parameters / fields to add in schedule
        List<BuiltInParameter> BiParams = new List<BuiltInParameter> { BuiltInParameter.SCHEDULE_CATEGORY };
        private readonly UIDocument uidoc;
        private readonly Document doc;
        public ScheduleGenerator(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
        }

        public TransactionStatus GenerateCategorySchedule(BuiltInCategory category, BuiltInParameter parameter = BuiltInParameter.INVALID)
        {
            TransactionStatus status = new TransactionStatus();
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Generating Schedule");
                if (parameter != BuiltInParameter.INVALID)
                    BiParams.Add(parameter);
                ElementId categoryId = new ElementId(category);
                ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, categoryId);
                schedule.Name = "Schedule";
                IList<SchedulableField> schedylableFields = schedule.Definition.GetSchedulableFields().ToList();
                ScheduleField scheduleField=default;
                foreach (SchedulableField schedylableField in schedylableFields)
                {
                    if (CheckField(schedylableField))
                    {
                        scheduleField = schedule.Definition.AddField(schedylableField);
                    }
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
    }
}
