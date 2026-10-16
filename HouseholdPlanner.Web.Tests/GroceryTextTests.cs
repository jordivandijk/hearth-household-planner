using HouseholdPlanner.Web.Models;
using HouseholdPlanner.Web.Services;

namespace HouseholdPlanner.Web.Tests;

public class GroceryTextTests
{
    [Theory]
    [InlineData("2 lemons", 2, "Lemons")]
    [InlineData("2x lemons", 2, "Lemons")]
    [InlineData("2 x lemons", 2, "Lemons")]
    [InlineData("12 eggs", 12, "Eggs")]
    [InlineData("  oat milk  ", 1, "Oat milk")]
    [InlineData("0 lemons", 1, "Lemons")]           // at least one
    [InlineData("7up", 1, "7up")]                   // a number that is part of the name
    [InlineData("2xl shirt", 1, "2xl shirt")]
    [InlineData("1000 lemons", 1, "1000 lemons")]   // more than the API's maximum: not a quantity
    [InlineData("999 lemons", 999, "Lemons")]
    public void Quick_add_reads_a_leading_quantity(string text, int quantity, string name)
        => Assert.Equal((quantity, name), GroceryText.ParseQuickAdd(text));

    [Theory]
    [InlineData("Oat milk", GroceryCategory.DairyEggs)]
    [InlineData("Avocados", GroceryCategory.Produce)]
    [InlineData("Sourdough loaf", GroceryCategory.Bakery)]
    [InlineData("Chicken thighs", GroceryCategory.MeatFish)]
    [InlineData("Basmati RICE", GroceryCategory.Pantry)]
    [InlineData("Frozen peas", GroceryCategory.Frozen)]
    [InlineData("Dish soap", GroceryCategory.Household)]
    [InlineData("Birthday candles", GroceryCategory.Other)]
    [InlineData("", GroceryCategory.Other)]
    public void Aisles_are_guessed_from_the_name(string name, GroceryCategory category)
        => Assert.Equal(category, GroceryText.GuessCategory(name));

    [Theory]
    [InlineData("Halfvolle melk", GroceryCategory.DairyEggs)]
    [InlineData("Kleine eieren", GroceryCategory.DairyEggs)]
    [InlineData("Ui", GroceryCategory.Produce)]
    [InlineData("Spinazie", GroceryCategory.Produce)]
    [InlineData("Volkoren brood", GroceryCategory.Bakery)]
    [InlineData("Kipfilet", GroceryCategory.MeatFish)]
    [InlineData("Rijst", GroceryCategory.Pantry)]
    [InlineData("Wasmiddel", GroceryCategory.Household)]
    public void Dutch_names_are_guessed_too(string name, GroceryCategory category)
        => Assert.Equal(category, GroceryText.GuessCategory(name));

    // The longest matching word wins, so a word inside a longer one doesn't decide.
    [Theory]
    [InlineData("Pindakaas", GroceryCategory.Pantry)]          // not cheese
    [InlineData("Tandpasta", GroceryCategory.Household)]       // not pasta
    [InlineData("Diepvriesspinazie", GroceryCategory.Frozen)]  // frozen beats spinach
    [InlineData("Melkchocolade", GroceryCategory.Pantry)]      // chocolate beats milk
    [InlineData("Ice cream", GroceryCategory.Frozen)]          // not cream
    public void The_longest_matching_word_wins(string name, GroceryCategory category)
        => Assert.Equal(category, GroceryText.GuessCategory(name));

    // Everyday items that hold a shorter keyword from another aisle.
    [Theory]
    [InlineData("Steak", GroceryCategory.MeatFish)]          // has "tea"
    [InlineData("Radishes", GroceryCategory.Produce)]        // has "dish"
    [InlineData("Eggplant", GroceryCategory.Produce)]        // has "egg"
    [InlineData("Green beans", GroceryCategory.Produce)]     // has "beans"
    [InlineData("Peanut butter", GroceryCategory.Pantry)]    // has "butter"
    [InlineData("Bloemkool", GroceryCategory.Produce)]       // has "bloem" (flour)
    [InlineData("Sperziebonen", GroceryCategory.Produce)]    // has "bonen"
    [InlineData("Ham", GroceryCategory.MeatFish)]
    [InlineData("Shampoo", GroceryCategory.Household)]       // has "ham"
    public void Words_inside_longer_names_do_not_mislead(string name, GroceryCategory category)
        => Assert.Equal(category, GroceryText.GuessCategory(name));

    // Very short words only count on their own, not inside other words.
    [Theory]
    [InlineData("Huishoudpapier", GroceryCategory.Household)]  // has "ui"
    [InlineData("Fruit", GroceryCategory.Produce)]              // has "ui", but is fruit
    [InlineData("Ijsblokjes", GroceryCategory.Other)]           // has "ijs", but isn't ice cream
    public void Short_words_only_count_as_whole_words(string name, GroceryCategory category)
        => Assert.Equal(category, GroceryText.GuessCategory(name));
}
