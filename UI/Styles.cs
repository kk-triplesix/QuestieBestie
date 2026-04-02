using System.Numerics;

namespace QuestieBestie.UI;

public static class Styles
{
    // Semantic accent colors (for inline text highlighting)
    public static readonly Vector4 AccentCyan = new(0.33f, 0.79f, 0.79f, 1.00f);    // #53c9c9
    public static readonly Vector4 AccentGreen = new(0.31f, 0.80f, 0.64f, 1.00f);   // #4ecca3

    // Semantic text colors (for inline highlighting, not theme overrides)
    public static readonly Vector4 TextPrimary = new(0.92f, 0.93f, 0.95f, 1.00f);   // #ebedf2
    public static readonly Vector4 TextSecondary = new(0.60f, 0.63f, 0.70f, 1.00f); // #99a0b3
    public static readonly Vector4 TextDimmed = new(0.40f, 0.42f, 0.48f, 1.00f);    // #666b7a
    public static readonly Vector4 TextGreen = new(0.31f, 0.80f, 0.64f, 1.00f);     // #4ecca3
    public static readonly Vector4 TextRed = new(0.90f, 0.35f, 0.40f, 1.00f);       // #e65966

    // Expansion colors
    public static readonly Vector4 ExpArr = new(0.35f, 0.55f, 0.90f, 1.00f);      // ARR blue
    public static readonly Vector4 ExpHw = new(0.40f, 0.80f, 0.90f, 1.00f);       // HW cyan
    public static readonly Vector4 ExpSb = new(0.90f, 0.40f, 0.40f, 1.00f);       // SB red
    public static readonly Vector4 ExpShb = new(0.70f, 0.45f, 0.85f, 1.00f);      // ShB purple
    public static readonly Vector4 ExpEw = new(0.90f, 0.80f, 0.35f, 1.00f);       // EW gold
    public static readonly Vector4 ExpDt = new(0.40f, 0.85f, 0.50f, 1.00f);       // DT green
    public static readonly Vector4 ExpOther = new(0.60f, 0.63f, 0.70f, 1.00f);    // fallback

    public static readonly Vector4 FavoriteStar = new(1.00f, 0.85f, 0.20f, 1.00f); // gold star

    // Progress bar background (specific element override, not theme)
    public static readonly Vector4 BgLight = new(0.12f, 0.16f, 0.28f, 1.00f);       // #1f2847

    // Shared expansion data
    public static readonly (uint Id, string Name)[] Expansions =
        [(0, "A Realm Reborn"), (1, "Heavensward"), (2, "Stormblood"), (3, "Shadowbringers"), (4, "Endwalker"), (5, "Dawntrail")];

    public static string GetExpansionAbbrev(uint expansionId) => expansionId switch
    {
        0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?"
    };

    public static Vector4 GetExpansionColor(uint expansionId)
    {
        return expansionId switch
        {
            0 => ExpArr,
            1 => ExpHw,
            2 => ExpSb,
            3 => ExpShb,
            4 => ExpEw,
            5 => ExpDt,
            _ => ExpOther,
        };
    }
}
