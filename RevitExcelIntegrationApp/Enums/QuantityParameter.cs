using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitExcelIntegrationApp.Enums
{
    public enum QuantityParameter
    {
        [Display(Name ="Count")]
        Count = 1,
        [Display(Name ="Length")]
        Length = 2,
        [Display(Name ="Area")]
        Area = 3,
        [Display(Name ="Volume")]
        Volume = 4
    }
}
