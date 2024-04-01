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
        private readonly List<BuiltInParameter> BiParams = new List<BuiltInParameter> { BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM };
        private readonly UIDocument uidoc;
        private readonly Document doc;
        public ScheduleGenerator(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
        }
        public bool GenerateCategorySchedule(BuiltInCategory category, string selectedParameter, string scheduleName)
        {
            TransactionStatus transactionStatus = new TransactionStatus();
            try
            {
                BiParams.Add(GettingBuiltInParameterBasedOnSelectedParameter(selectedParameter));
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Generating Schedule");
                    ElementId categoryId = new ElementId(category);
                    ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, categoryId);
                    schedule.Name = scheduleName;
                    IList<SchedulableField> schedylableFields = schedule.Definition.GetSchedulableFields().ToList();
                    foreach (SchedulableField schedylableField in schedylableFields)
                        if (CheckField(schedylableField))
                            schedule.Definition.AddField(schedylableField);
                    CalculateCategoryTotalPrice(category, selectedParameter);
                    var priceGUID = GetParameterID(category, Constants.Price);
                    var SharedParameterPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == priceGUID);
                    var totalPriceGUID = GetParameterID(category, Constants.TotalPrice);
                    var SharedParametertotalPriceField = schedylableFields.FirstOrDefault(x => x.ParameterId == totalPriceGUID);
                    schedule.Definition.AddField(SharedParameterPriceField);
                    schedule.Definition.AddField(SharedParametertotalPriceField);
                    transactionStatus = t.Commit();
                    uidoc.ActiveView = schedule;
                }
            }
            catch (Exception)
            {
                throw;
            }
            if (transactionStatus == TransactionStatus.Committed)
                return true;
            else
                return false;
        }

        private static BuiltInParameter GettingBuiltInParameterBasedOnSelectedParameter(string selectedParameter)
        {
            QuantityParameter selectedQuantityParameter = (QuantityParameter)Enum.Parse(typeof(QuantityParameter), selectedParameter);
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
            return builtInParameter;
        }

        private bool CheckField(SchedulableField schedulableField)
        {
            foreach (BuiltInParameter bip in BiParams)
                if (new ElementId(bip) == schedulableField.ParameterId)
                    return true;
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
            var elements = Utilities.GetAllStructuralGraphicalElements(doc, category);
            foreach (var element in elements)
            {
                var totalPriceParameter = element.LookupParameter(Constants.TotalPrice);
                var priceParameter = element.LookupParameter(Constants.Price).AsDouble();
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
        }
    }
}

