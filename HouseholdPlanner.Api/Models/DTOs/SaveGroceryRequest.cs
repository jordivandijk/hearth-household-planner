using System.ComponentModel.DataAnnotations;
using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Models.DTOs;

/// <summary>Adds an item, or edits one when sent to its id.</summary>
public class SaveGroceryRequest
{
    [Required, StringLength(100, MinimumLength = 1)] public string Name { get; set; } = string.Empty;
    [Range(1, GroceryItem.MaxQuantity)] public int Quantity { get; set; } = 1;
    [EnumDataType(typeof(GroceryCategory))] public GroceryCategory Category { get; set; } = GroceryCategory.Other;
    public bool InCart { get; set; }
}
