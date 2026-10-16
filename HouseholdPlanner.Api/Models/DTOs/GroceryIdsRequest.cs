using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Api.Models.DTOs;

public class GroceryIdsRequest
{
    [Required, MaxLength(500)] public List<Guid> Ids { get; set; } = [];
}
