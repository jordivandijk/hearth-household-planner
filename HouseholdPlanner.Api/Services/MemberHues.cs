namespace HouseholdPlanner.Api.Services;

/// <summary>Picks avatar colours that tell household members apart.</summary>
public static class MemberHues
{
    private static readonly int[] Palette = [35, 250, 150, 300, 200, 90, 330, 15];

    /// <summary>The first palette hue nobody in the household has yet, or a cycle through the palette once all are taken.</summary>
    public static int Next(IReadOnlyCollection<int> taken)
        => Palette.FirstOrDefault(h => !taken.Contains(h), Palette[taken.Count % Palette.Length]);
}
