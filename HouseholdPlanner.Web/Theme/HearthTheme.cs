using MudBlazor;

namespace HouseholdPlanner.Web.Theme;

/// <summary>
/// The Hearth look (warm neutrals, terracotta accent, rounded Quicksand type) as a MudBlazor theme,
/// so components get it without per-page CSS. Secondary is the ink colour, for selected chips and dark buttons.
/// </summary>
public static class HearthTheme
{
    public const string Accent = "#c25237";

    private static readonly string[] Font = ["Quicksand", "system-ui", "sans-serif"];

    public static MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary          = Accent,
            Secondary        = "#2a2420",
            SecondaryContrastText = "#f6f3ee",
            Dark             = "#2a2420",  // toasts
            Tertiary         = "#aa442b",
            Error            = "#a8372a",
            Background       = "#f6f3ee",
            BackgroundGray   = "#ebe5dd",
            Surface          = "#fffdf9",
            AppbarBackground = "rgba(246,243,238,.92)",
            AppbarText       = "#2a2420",
            DrawerBackground = "#fffdf9",
            TextPrimary      = "#2a2420",
            TextSecondary    = "#7a6e64",
            TextDisabled     = "#a1958a",
            ActionDefault    = "#7a6e64",
            LinesDefault     = "rgba(42,36,32,.08)",
            LinesInputs      = "rgba(42,36,32,.15)",
            Divider          = "rgba(42,36,32,.08)",
            Skeleton         = "#f1ebe3",
        },
        PaletteDark = new PaletteDark
        {
            Primary          = Accent,
            Secondary        = "#f1ebe4",
            SecondaryContrastText = "#1d1916",
            Dark             = "#423a34",  // toasts
            Tertiary         = "#f3947c",
            Error            = "#e0705f",
            Background       = "#1d1916",
            BackgroundGray   = "#36302b",
            Surface          = "#28231f",
            AppbarBackground = "rgba(29,25,22,.9)",
            AppbarText       = "#f1ebe4",
            DrawerBackground = "#28231f",
            TextPrimary      = "#f1ebe4",
            TextSecondary    = "#ab9f94",
            TextDisabled     = "#7f746a",
            ActionDefault    = "#ab9f94",
            LinesDefault     = "rgba(241,235,228,.08)",
            LinesInputs      = "rgba(241,235,228,.17)",
            Divider          = "rgba(241,235,228,.08)",
            Skeleton         = "#322c27",
        },
        Typography = new Typography
        {
            Default   = new DefaultTypography { FontFamily = Font, FontWeight = "500" },
            H4        = new H4Typography { FontFamily = Font, FontWeight = "700", FontSize = "2.125rem", LineHeight = "1.05", LetterSpacing = "-0.025em" },
            H5        = new H5Typography { FontFamily = Font, FontWeight = "700", FontSize = "1.375rem" },
            H6        = new H6Typography { FontFamily = Font, FontWeight = "700", FontSize = "1.125rem" },
            Subtitle1 = new Subtitle1Typography { FontFamily = Font, FontWeight = "500", FontSize = ".9375rem" },
            Subtitle2 = new Subtitle2Typography { FontFamily = Font, FontWeight = "600", FontSize = ".8125rem" },
            Body1     = new Body1Typography { FontFamily = Font, FontWeight = "600", FontSize = "1rem", LineHeight = "1.3" },
            Body2     = new Body2Typography { FontFamily = Font, FontWeight = "500", FontSize = ".9375rem" },
            Caption   = new CaptionTypography { FontFamily = Font, FontWeight = "600", FontSize = ".8125rem" },
            Overline  = new OverlineTypography { FontFamily = Font, FontWeight = "700", FontSize = ".75rem", LetterSpacing = ".06em", LineHeight = "1.5" },
            Button    = new ButtonTypography { FontFamily = Font, FontWeight = "700", TextTransform = "none", LetterSpacing = "0" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "16px",
        }
    };
}
