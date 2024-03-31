using System.ComponentModel.DataAnnotations;

namespace RevitExcelIntegrationApp.Enums
{
    public enum QuantityParameter
    {
        [Display(Name ="Length")]
        Length = 1,
        [Display(Name ="Area")]
        Area = 2,
        [Display(Name ="Volume")]
        Volume = 3
    }
}
