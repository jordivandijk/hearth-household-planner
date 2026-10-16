using System.ComponentModel.DataAnnotations;
using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Tests;

public class RequestValidationTests
{
    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Task_with_a_title_is_valid()
        => Assert.Empty(Validate(new SaveTaskRequest { Title = "Vacuum", Repeat = TaskRepeat.Weekly }));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Task_needs_a_title(string title)
        => Assert.NotEmpty(Validate(new SaveTaskRequest { Title = title }));

    // The JSON enum converter also accepts numbers, so 42 would otherwise get stored.
    [Fact]
    public void Task_repeat_must_be_a_known_value()
        => Assert.NotEmpty(Validate(new SaveTaskRequest { Title = "Vacuum", Repeat = (TaskRepeat)42 }));

    [Fact]
    public void Task_can_repeat_every_two_weeks()
        => Assert.Empty(Validate(new SaveTaskRequest { Title = "Change bed sheets", Repeat = TaskRepeat.Biweekly }));

    [Fact]
    public void Week_start_must_be_a_day_of_the_week()
    {
        Assert.Empty(Validate(new SetWeekStartRequest { WeekStartsOn = DayOfWeek.Sunday }));
        Assert.NotEmpty(Validate(new SetWeekStartRequest { WeekStartsOn = (DayOfWeek)7 }));
        Assert.NotEmpty(Validate(new SetWeekStartRequest()));
    }

    [Fact]
    public void Grocery_category_must_be_a_known_value()
        => Assert.NotEmpty(Validate(new SaveGroceryRequest { Name = "Eggs", Category = (GroceryCategory)42 }));

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(GroceryItem.MaxQuantity, true)]
    [InlineData(GroceryItem.MaxQuantity + 1, false)]
    public void Grocery_quantity_is_between_1_and_the_maximum(int quantity, bool valid)
        => Assert.Equal(valid, Validate(new SaveGroceryRequest { Name = "Eggs", Quantity = quantity }).Count == 0);

    [Theory]
    [InlineData("J", "secret1", false)]
    [InlineData("June", "short", false)]
    [InlineData("June", "secret1", true)]
    public void Registering_needs_a_name_of_2_and_a_password_of_6(string username, string password, bool valid)
        => Assert.Equal(valid, Validate(new RegisterRequest { InviteCode = "KQTM-4821", Username = username, Password = password }).Count == 0);

    [Fact]
    public void Restoring_takes_a_bounded_number_of_ids()
        => Assert.NotEmpty(Validate(new GroceryIdsRequest { Ids = Enumerable.Range(0, 501).Select(_ => Guid.NewGuid()).ToList() }));
}
