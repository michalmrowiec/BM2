using System.ComponentModel.DataAnnotations;

namespace BM2.Shared.SystemCodes;

public enum Periodicity
{
    //Daily = 1,
    //Weekly = 2,
    [Display(Name = "Monthly")]
    Monthly = 3,
    [Display(Name = "Yearly")]
    Yearly = 4,
    //Custom = 99
}