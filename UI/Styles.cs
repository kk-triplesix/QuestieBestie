using System.Numerics;
using Dalamud.Interface.Utility;

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
    public static readonly Vector4 ExpArr = new(0.35f, 0.55f, 0.90f, 1.00f);
    public static readonly Vector4 ExpHw = new(0.40f, 0.80f, 0.90f, 1.00f);
    public static readonly Vector4 ExpSb = new(0.90f, 0.40f, 0.40f, 1.00f);
    public static readonly Vector4 ExpShb = new(0.70f, 0.45f, 0.85f, 1.00f);
    public static readonly Vector4 ExpEw = new(0.90f, 0.80f, 0.35f, 1.00f);
    public static readonly Vector4 ExpDt = new(0.40f, 0.85f, 0.50f, 1.00f);
    public static readonly Vector4 ExpOther = new(0.60f, 0.63f, 0.70f, 1.00f);

    public static readonly Vector4 FavoriteStar = new(1.00f, 0.85f, 0.20f, 1.00f);

    // Progress bar background
    public static readonly Vector4 BgLight = new(0.12f, 0.16f, 0.28f, 1.00f);

    // Shared expansion data
    public static readonly (uint Id, string Name)[] Expansions =
        [(0, "A Realm Reborn"), (1, "Heavensward"), (2, "Stormblood"), (3, "Shadowbringers"), (4, "Endwalker"), (5, "Dawntrail")];

    public static string GetExpansionAbbrev(uint expansionId) => expansionId switch
    {
        0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?"
    };

    public static Vector4 GetExpansionColor(uint expansionId) => expansionId switch
    {
        0 => ExpArr, 1 => ExpHw, 2 => ExpSb, 3 => ExpShb, 4 => ExpEw, 5 => ExpDt, _ => ExpOther,
    };

    // ── Optional custom theme (off by default, user opt-in) ────────────

    private const int CustomStyleCount = 9;
    private const int CustomColorCount = 31;

    public static void PushCustomTheme()
    {
        var s = ImGuiHelpers.GlobalScale;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 8.0f * s);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f * s);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4.0f * s);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 4.0f * s);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 4.0f * s);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12, 12) * s);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 4) * s);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6) * s);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(6, 4) * s);

        Col(ImGuiCol.WindowBg,           0.10f, 0.10f, 0.18f, 1.00f);
        Col(ImGuiCol.ChildBg,            0.09f, 0.13f, 0.24f, 1.00f);
        Col(ImGuiCol.PopupBg,            0.09f, 0.13f, 0.24f, 1.00f);
        Col(ImGuiCol.Border,             0.18f, 0.22f, 0.34f, 1.00f);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, BgLight);
        Col(ImGuiCol.FrameBgHovered,     0.06f, 0.20f, 0.38f, 1.00f);
        Col(ImGuiCol.FrameBgActive,      0.06f, 0.20f, 0.38f, 1.00f);
        Col(ImGuiCol.TitleBg,            0.10f, 0.10f, 0.18f, 1.00f);
        Col(ImGuiCol.TitleBgActive,      0.09f, 0.13f, 0.24f, 1.00f);
        Col(ImGuiCol.TitleBgCollapsed,   0.10f, 0.10f, 0.18f, 1.00f);
        Col(ImGuiCol.ScrollbarBg,        0.08f, 0.10f, 0.17f, 1.00f);
        Col(ImGuiCol.ScrollbarGrab,      0.06f, 0.20f, 0.38f, 1.00f);
        Col(ImGuiCol.ScrollbarGrabHovered, 0.10f, 0.28f, 0.48f, 1.00f);
        Col(ImGuiCol.ScrollbarGrabActive, 0.15f, 0.35f, 0.55f, 1.00f);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, AccentCyan);
        Col(ImGuiCol.Button,             0.06f, 0.20f, 0.38f, 1.00f);
        Col(ImGuiCol.ButtonHovered,      0.10f, 0.28f, 0.48f, 1.00f);
        Col(ImGuiCol.ButtonActive,       0.15f, 0.35f, 0.55f, 1.00f);
        Col(ImGuiCol.Header,             0.06f, 0.20f, 0.38f, 1.00f);
        Col(ImGuiCol.HeaderHovered,      0.10f, 0.28f, 0.48f, 1.00f);
        Col(ImGuiCol.HeaderActive,       0.15f, 0.35f, 0.55f, 1.00f);
        Col(ImGuiCol.Tab,                0.09f, 0.13f, 0.24f, 1.00f);
        Col(ImGuiCol.TabHovered,         0.06f, 0.20f, 0.38f, 1.00f);
        Col(ImGuiCol.TableHeaderBg,      0.08f, 0.11f, 0.20f, 1.00f);
        Col(ImGuiCol.TableBorderStrong,  0.18f, 0.22f, 0.34f, 1.00f);
        Col(ImGuiCol.TableBorderLight,   0.18f, 0.22f, 0.34f, 0.40f);
        Col(ImGuiCol.TableRowBgAlt,      0.11f, 0.14f, 0.24f, 0.50f);
        ImGui.PushStyleColor(ImGuiCol.Text, TextPrimary);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, TextDimmed);
        Col(ImGuiCol.Separator,          0.18f, 0.22f, 0.34f, 1.00f);
        ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, AccentCyan);
    }

    public static void PopCustomTheme()
    {
        ImGui.PopStyleColor(CustomColorCount);
        ImGui.PopStyleVar(CustomStyleCount);
    }

    private static void Col(ImGuiCol target, float r, float g, float b, float a)
        => ImGui.PushStyleColor(target, new Vector4(r, g, b, a));
}
