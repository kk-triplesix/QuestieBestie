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

    private static int _customStyleCount;
    private static int _customColorCount;

    public static void PushCustomTheme()
    {
        _customStyleCount = 0;
        _customColorCount = 0;
        var s = ImGuiHelpers.GlobalScale;

        Push(ImGuiStyleVar.WindowRounding, 8.0f * s);
        Push(ImGuiStyleVar.FrameRounding, 4.0f * s);
        Push(ImGuiStyleVar.GrabRounding, 4.0f * s);
        Push(ImGuiStyleVar.ScrollbarRounding, 4.0f * s);
        Push(ImGuiStyleVar.TabRounding, 4.0f * s);
        Push(ImGuiStyleVar.WindowPadding, new Vector2(12, 12) * s);
        Push(ImGuiStyleVar.FramePadding, new Vector2(8, 4) * s);
        Push(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6) * s);
        Push(ImGuiStyleVar.CellPadding, new Vector2(6, 4) * s);

        PushC(ImGuiCol.WindowBg, new(0.10f, 0.10f, 0.18f, 1.00f));
        PushC(ImGuiCol.ChildBg, new(0.09f, 0.13f, 0.24f, 1.00f));
        PushC(ImGuiCol.PopupBg, new(0.09f, 0.13f, 0.24f, 1.00f));
        PushC(ImGuiCol.Border, new(0.18f, 0.22f, 0.34f, 1.00f));
        PushC(ImGuiCol.FrameBg, BgLight);
        PushC(ImGuiCol.FrameBgHovered, new(0.06f, 0.20f, 0.38f, 1.00f));
        PushC(ImGuiCol.FrameBgActive, new(0.06f, 0.20f, 0.38f, 1.00f));
        PushC(ImGuiCol.TitleBg, new(0.10f, 0.10f, 0.18f, 1.00f));
        PushC(ImGuiCol.TitleBgActive, new(0.09f, 0.13f, 0.24f, 1.00f));
        PushC(ImGuiCol.TitleBgCollapsed, new(0.10f, 0.10f, 0.18f, 1.00f));
        PushC(ImGuiCol.ScrollbarBg, new(0.08f, 0.10f, 0.17f, 1.00f));
        PushC(ImGuiCol.ScrollbarGrab, new(0.06f, 0.20f, 0.38f, 1.00f));
        PushC(ImGuiCol.ScrollbarGrabHovered, new(0.10f, 0.28f, 0.48f, 1.00f));
        PushC(ImGuiCol.ScrollbarGrabActive, new(0.15f, 0.35f, 0.55f, 1.00f));
        PushC(ImGuiCol.CheckMark, AccentCyan);
        PushC(ImGuiCol.Button, new(0.06f, 0.20f, 0.38f, 1.00f));
        PushC(ImGuiCol.ButtonHovered, new(0.10f, 0.28f, 0.48f, 1.00f));
        PushC(ImGuiCol.ButtonActive, new(0.15f, 0.35f, 0.55f, 1.00f));
        PushC(ImGuiCol.Header, new(0.06f, 0.20f, 0.38f, 1.00f));
        PushC(ImGuiCol.HeaderHovered, new(0.10f, 0.28f, 0.48f, 1.00f));
        PushC(ImGuiCol.HeaderActive, new(0.15f, 0.35f, 0.55f, 1.00f));
        PushC(ImGuiCol.Tab, new(0.09f, 0.13f, 0.24f, 1.00f));
        PushC(ImGuiCol.TabHovered, new(0.06f, 0.20f, 0.38f, 1.00f));
        PushC(ImGuiCol.TableHeaderBg, new(0.08f, 0.11f, 0.20f, 1.00f));
        PushC(ImGuiCol.TableBorderStrong, new(0.18f, 0.22f, 0.34f, 1.00f));
        PushC(ImGuiCol.TableBorderLight, new(0.18f, 0.22f, 0.34f, 0.40f));
        PushC(ImGuiCol.TableRowBgAlt, new(0.11f, 0.14f, 0.24f, 0.50f));
        PushC(ImGuiCol.Text, TextPrimary);
        PushC(ImGuiCol.TextDisabled, TextDimmed);
        PushC(ImGuiCol.Separator, new(0.18f, 0.22f, 0.34f, 1.00f));
        PushC(ImGuiCol.SeparatorHovered, AccentCyan);
    }

    public static void PopCustomTheme()
    {
        ImGui.PopStyleColor(_customColorCount);
        ImGui.PopStyleVar(_customStyleCount);
    }

    private static void Push(ImGuiStyleVar v, float val) { ImGui.PushStyleVar(v, val); _customStyleCount++; }
    private static void Push(ImGuiStyleVar v, Vector2 val) { ImGui.PushStyleVar(v, val); _customStyleCount++; }
    private static void PushC(ImGuiCol c, Vector4 val) { ImGui.PushStyleColor(c, val); _customColorCount++; }
}
