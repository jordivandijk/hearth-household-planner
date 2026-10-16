using System.Text.RegularExpressions;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.Services;

/// <summary>Reading what people type into the grocery list.</summary>
public static partial class GroceryText
{
    // English and Dutch together: one shared list can have both, whatever language each person uses.
    // A keyword matches anywhere in the name ("milk" in "oat milk"); when several match, the longest wins,
    // so "pindakaas" is pantry rather than cheese and "tandpasta" household rather than pasta.
    private static readonly (GroceryCategory Category, string[] Words)[] Keywords =
    [
        (GroceryCategory.Produce, ["apple", "banana", "avocado", "lemon", "lime", "onion", "garlic", "tomato", "lettuce", "spinach", "carrot", "potato", "berr", "pepper", "cucumber", "herb", "basil", "cilantro", "fruit", "kale", "ginger", "mushroom", "grape", "orange", "radish", "eggplant", "aubergine", "green bean", "cauliflower", "zucchini",
                                   "appel", "banaan", "bananen", "citroen", "limoen", "uien", "knoflook", "tomaat", "tomaten", "spinazie", "wortel", "aardappel", "bessen", "aardbei", "paprika", "komkommer", "basilicum", "koriander", "boerenkool", "gember", "champignon", "druif", "druiven", "sinaasappel", "peer", "peren", "courgette", "broccoli", "prei", "bloemkool", "sperziebonen", "spruitjes"]),
        (GroceryCategory.DairyEggs, ["milk", "egg", "cheese", "cheddar", "yogurt", "butter", "cream", "feta", "parmesan",
                                     "melk", "eieren", "kaas", "yoghurt", "boter", "slagroom", "room", "parmezaan", "kwark", "vla"]),
        (GroceryCategory.Bakery, ["bread", "bagel", "tortilla", "bun", "croissant", "pita", "sourdough", "loaf",
                                  "brood", "bolletje", "beschuit", "krentenbol", "wraps"]),
        (GroceryCategory.MeatFish, ["chicken", "steak", "beef", "pork", "salmon", "fish", "tofu", "turkey", "bacon", "sausage", "shrimp", "mince",
                                    "kip", "rundvlees", "varkensvlees", "zalm", "tonijn", "kalkoen", "spek", "worst", "garnalen", "gehakt"]),
        (GroceryCategory.Frozen, ["frozen", "ice cream", "peas", "pizza",
                                  "diepvries", "roomijs", "ijsjes", "erwten", "friet"]),
        (GroceryCategory.Pantry, ["rice", "pasta", "oil", "flour", "sugar", "coffee", "tea", "beans", "salt", "sauce", "cereal", "oats", "honey", "spice", "vinegar", "stock", "nuts", "peanut butter", "chocolate", "crackers",
                                  "rijst", "spaghetti", "macaroni", "olie", "meel", "bloem", "suiker", "koffie", "thee", "bonen", "zout", "saus", "ketchup", "mayonaise", "mosterd", "muesli", "havermout", "honing", "azijn", "bouillon", "noten", "pinda", "chocola", "pindakaas", "hagelslag", "jam", "soep"]),
        (GroceryCategory.Household, ["soap", "detergent", "paper", "towel", "trash", "bag", "sponge", "toothpaste", "shampoo", "foil", "batteries", "bulb", "dish", "tissue",
                                     "zeep", "wasmiddel", "papier", "handdoek", "vuilniszak", "spons", "tandpasta", "folie", "batterij", "lamp", "afwas", "schoonmaak", "allesreiniger"]),
    ];

    // Words too short to look for inside other words ("ui" is in "fruit", "ham" in "shampoo"): only whole words count.
    private static readonly (GroceryCategory Category, string[] Words)[] WholeWords =
    [
        (GroceryCategory.Produce, ["ui", "sla"]),
        (GroceryCategory.DairyEggs, ["ei"]),
        (GroceryCategory.MeatFish, ["vis", "ham"]),
        (GroceryCategory.Frozen, ["ijs"]),
    ];

    private static readonly char[] WordBreaks = [' ', '-', ',', '.', '/'];

    /// <summary>The aisle an item is probably in, from words in its name.</summary>
    public static GroceryCategory GuessCategory(string name)
    {
        var lower = name.ToLowerInvariant();
        var words = lower.Split(WordBreaks, StringSplitOptions.RemoveEmptyEntries);

        var matches = Keywords.SelectMany(k => k.Words.Where(lower.Contains).Select(w => (k.Category, w.Length)))
            .Concat(WholeWords.SelectMany(k => k.Words.Where(words.Contains).Select(w => (k.Category, w.Length))));

        return matches.OrderByDescending(m => m.Length).Select(m => m.Category).DefaultIfEmpty(GroceryCategory.Other).First();
    }

    /// <summary>"2 lemons" or "2x lemons" → (2, "Lemons"); anything else is one of it.</summary>
    public static (int Quantity, string Name) ParseQuickAdd(string text)
    {
        text = text.Trim();
        var match = QuantityPrefix().Match(text);
        var (quantity, name) = match.Success
            ? (Math.Max(1, int.Parse(match.Groups[1].Value)), match.Groups[2].Value)
            : (1, text);
        return (quantity, Capitalize(name));
    }

    public static string Capitalize(string name)
        => name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name[1..];

    [GeneratedRegex(@"^(\d{1,3})\s*x?\s+(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityPrefix();
}
