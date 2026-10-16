using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Api.Models.DTOs;

public class SetWeekStartRequest
{
    /// <summary>A day by name, like "Monday".</summary>
    [Required, EnumDataType(typeof(DayOfWeek))] public DayOfWeek? WeekStartsOn { get; set; }
}
