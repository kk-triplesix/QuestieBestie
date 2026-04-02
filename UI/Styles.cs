using System.Numerics;

namespace QuestieBestie.UI;

public static class Styles
{
    // Background colors
    public static readonly Vector4 BgDark = new(0.10f, 0.10f, 0.18f, 1.00f);       // #1a1a2e
    public static readonly Vector4 BgMedium = new(0.09f, 0.13f, 0.24f, 1.00f);      // #16213e
    public static readonly Vector4 BgLight = new(0.12f, 0.16f, 0.28f, 1.00f);       // #1f2847

    // Accent colors
    public static readonly Vector4 AccentBlue = new(0.06f, 0.20f, 0.38f, 1.00f);    // #0f3460
    public static readonly Vector4 AccentCyan = new(0.33f, 0.79f, 0.79f, 1.00f);    // #53c9c9
    public static readonly Vector4 AccentGreen = new(0.31f, 0.80f, 0.64f, 1.00f);   // #4ecca3

    // Text colors
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

    // UI element colors
    public static readonly Vector4 BorderColor = new(0.18f, 0.22f, 0.34f, 1.00f);   // #2e3857
    public static readonly Vector4 HeaderBg = new(0.08f, 0.11f, 0.20f, 1.00f);      // #141c33
    public static readonly Vector4 RowBgAlt = new(0.11f, 0.14f, 0.24f, 0.50f);      // #1c243d (50%)
    public static readonly Vector4 ScrollbarBg = new(0.08f, 0.10f, 0.17f, 1.00f);   // #141a2b
    public static readonly Vector4 ButtonBg = new(0.06f, 0.20f, 0.38f, 1.00f);      // #0f3460
    public static readonly Vector4 ButtonHover = new(0.10f, 0.28f, 0.48f, 1.00f);   // #1a477a
    public static readonly Vector4 ButtonActive = new(0.15f, 0.35f, 0.55f, 1.00f);  // #26598c

    // Overlay
    public static readonly Vector4 OverlayBg = new(0.08f, 0.08f, 0.14f, 0.85f);    // semi-transparent dark

    private static readonly Stack<(int Styles, int Colors)> StyleStack = new();
    private static int _styleCount;
    private static int _colorCount;

    public static void PushMainStyle()
    {
        _styleCount = 0;
        _colorCount = 0;

        PushStyleVar(ImGuiStyleVar.WindowRounding, 8.0f);
        PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f);
        PushStyleVar(ImGuiStyleVar.GrabRounding, 4.0f);
        PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 4.0f);
        PushStyleVar(ImGuiStyleVar.TabRounding, 4.0f);
        PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12, 12));
        PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 4));
        PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6));
        PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(6, 4));

        PushColor(ImGuiCol.WindowBg, BgDark);
        PushColor(ImGuiCol.ChildBg, BgMedium);
        PushColor(ImGuiCol.PopupBg, BgMedium);
        PushColor(ImGuiCol.Border, BorderColor);
        PushColor(ImGuiCol.FrameBg, BgLight);
        PushColor(ImGuiCol.FrameBgHovered, AccentBlue);
        PushColor(ImGuiCol.FrameBgActive, AccentBlue);
        PushColor(ImGuiCol.TitleBg, BgDark);
        PushColor(ImGuiCol.TitleBgActive, BgMedium);
        PushColor(ImGuiCol.TitleBgCollapsed, BgDark);
        PushColor(ImGuiCol.ScrollbarBg, ScrollbarBg);
        PushColor(ImGuiCol.ScrollbarGrab, AccentBlue);
        PushColor(ImGuiCol.ScrollbarGrabHovered, ButtonHover);
        PushColor(ImGuiCol.ScrollbarGrabActive, ButtonActive);
        PushColor(ImGuiCol.CheckMark, AccentCyan);
        PushColor(ImGuiCol.Button, ButtonBg);
        PushColor(ImGuiCol.ButtonHovered, ButtonHover);
        PushColor(ImGuiCol.ButtonActive, ButtonActive);
        PushColor(ImGuiCol.Header, AccentBlue);
        PushColor(ImGuiCol.HeaderHovered, ButtonHover);
        PushColor(ImGuiCol.HeaderActive, ButtonActive);
        PushColor(ImGuiCol.Tab, BgMedium);
        PushColor(ImGuiCol.TabHovered, AccentBlue);
        PushColor(ImGuiCol.TableHeaderBg, HeaderBg);
        PushColor(ImGuiCol.TableBorderStrong, BorderColor);
        PushColor(ImGuiCol.TableBorderLight, new Vector4(BorderColor.X, BorderColor.Y, BorderColor.Z, 0.4f));
        PushColor(ImGuiCol.TableRowBgAlt, RowBgAlt);
        PushColor(ImGuiCol.Text, TextPrimary);
        PushColor(ImGuiCol.TextDisabled, TextDimmed);
        PushColor(ImGuiCol.Separator, BorderColor);
        PushColor(ImGuiCol.SeparatorHovered, AccentCyan);

        StyleStack.Push((_styleCount, _colorCount));
    }

    public static void PopMainStyle()
    {
        var (styles, colors) = StyleStack.Pop();
        ImGui.PopStyleColor(colors);
        ImGui.PopStyleVar(styles);
    }

    public static void PushOverlayStyle()
    {
        _styleCount = 0;
        _colorCount = 0;

        PushStyleVar(ImGuiStyleVar.WindowRounding, 10.0f);
        PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14, 10));
        PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 4));

        PushColor(ImGuiCol.WindowBg, OverlayBg);
        PushColor(ImGuiCol.Border, new Vector4(AccentCyan.X, AccentCyan.Y, AccentCyan.Z, 0.3f));
        PushColor(ImGuiCol.Text, TextPrimary);
        PushColor(ImGuiCol.TextDisabled, TextDimmed);

        StyleStack.Push((_styleCount, _colorCount));
    }

    public static void PopOverlayStyle()
    {
        var (styles, colors) = StyleStack.Pop();
        ImGui.PopStyleColor(colors);
        ImGui.PopStyleVar(styles);
    }

    private static void PushStyleVar(ImGuiStyleVar var, float val)
    {
        ImGui.PushStyleVar(var, val);
        _styleCount++;
    }

    private static void PushStyleVar(ImGuiStyleVar var, Vector2 val)
    {
        ImGui.PushStyleVar(var, val);
        _styleCount++;
    }

    private static void PushColor(ImGuiCol col, Vector4 val)
    {
        ImGui.PushStyleColor(col, val);
        _colorCount++;
    }
}
