using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class WidgetWindow : Window, IDisposable
{
    private readonly QuestService _questService;
    private readonly TrackingService _trackingService;
    private DateTime _lastHovered = DateTime.MinValue;

    public WidgetWindow(QuestService questService, TrackingService trackingService)
        : base("##QBWidget",
            ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoBringToFrontOnFocus)
    {
        _questService = questService;
        _trackingService = trackingService;
        IsOpen = false;
        RespectCloseHotkey = false;
        AllowClickthrough = false;
        AllowPinning = false;
        ShowCloseButton = false;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        var s = _trackingService.OverlaySettings;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, s.WindowRounding * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 6) * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 3) * ImGuiHelpers.GlobalScale);
        var bg = new Vector4(s.BackgroundColor.X, s.BackgroundColor.Y, s.BackgroundColor.Z, s.BackgroundAlpha);
        var border = new Vector4(s.BorderColor.X, s.BorderColor.Y, s.BorderColor.Z, s.BorderAlpha);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, bg);
        ImGui.PushStyleColor(ImGuiCol.Border, border);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(2);
        ImGui.PopStyleVar(3);
    }

    public override void Draw()
    {
        _questService.RefreshCompletionStatus();
        var s = _trackingService.OverlaySettings;

        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem))
            _lastHovered = DateTime.Now;
        var showBtn = (DateTime.Now - _lastHovered).TotalSeconds < 2.0;

        // Total progress (if enabled)
        if (s.WidgetShowTotal)
            DrawBar("Total", _questService.CompletedCount, _questService.TotalCount, s.CompletedColor, s);

        // Per-expansion progress bars (only iterate selected expansions)
        foreach (var (expId, expName) in Styles.Expansions)
        {
            if (!s.WidgetExpansions.Contains(expId))
                continue;

            var stat = _questService.GetExpansionStats().FirstOrDefault(st => st.Name == expName);
            if (stat == null)
                continue;

            DrawBar(Styles.GetExpansionAbbrev(expId), stat.Completed, stat.Total, Styles.GetExpansionColor(expId), s);
        }

        // Direction arrow
        var activeList = _trackingService.ActiveList;
        var direction = _questService.GetNearestTrackedQuestDirection(activeList.QuestRowIds);
        if (direction.HasValue)
        {
            var (angle, dist, name) = direction.Value;
            var arrow = GetDirectionArrow(angle);
            ImGui.PushStyleColor(ImGuiCol.Text, s.LevelColor);
            ImGui.Text($"{arrow} {name} ({dist:F0}y)");
            ImGui.PopStyleColor();
        }

        // Config + Close buttons — only show on hover, but ALWAYS render popup
        if (showBtn)
        {
            if (ImGuiComponents.IconButton("wCfg", FontAwesomeIcon.Cog))
                ImGui.OpenPopup("##widgetCfg");
            if (ImGui.IsItemHovered())
            { using var tt = ImRaii.Tooltip(); if (tt.Success) ImGui.Text(Loc.Get("header.settings")); }

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextRed);
            if (ImGuiComponents.IconButton("wClose", FontAwesomeIcon.Times))
                IsOpen = false;
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
            { using var tt = ImRaii.Tooltip(); if (tt.Success) ImGui.Text(Loc.Get("misc.close")); }
        }

        // Popup must be rendered outside the hover check — ImGui manages its open state
        DrawConfigPopup();
    }

    private static void DrawBar(string label, int completed, int total, Vector4 color, Models.OverlaySettings s)
    {
        var fraction = total > 0 ? (float)completed / total : 0f;

        ImGui.PushStyleColor(ImGuiCol.Text, s.HeaderColor);
        ImGui.Text(label);
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.SetCursorPosX(80 * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, color);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles.BgLight);
        ImGui.ProgressBar(fraction, new Vector2(100, 14) * ImGuiHelpers.GlobalScale, "");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();
        ImGui.SetCursorPosX(190 * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleColor(ImGuiCol.Text, s.TextColor);
        ImGui.Text($"{fraction * 100f:F0}%");
        ImGui.PopStyleColor();
    }

    private void DrawConfigPopup()
    {
        using var popup = ImRaii.Popup("##widgetCfg");
        if (!popup.Success)
            return;

        var s = _trackingService.OverlaySettings;
        var changed = false;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text(Loc.Get("settings.widgetBarsCfg"));
        ImGui.PopStyleColor();
        ImGui.Separator();

        // Total toggle
        if (ImGui.Checkbox("Total", ref s.WidgetShowTotal))
            changed = true;

        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text(Loc.Get("settings.expansions"));
        ImGui.PopStyleColor();

        // Expansion toggles
        foreach (var (id, name) in Styles.Expansions)
        {
            var enabled = s.WidgetExpansions.Contains(id);
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.GetExpansionColor(id));
            if (ImGui.Checkbox($"{name}###wExp{id}", ref enabled))
            {
                if (enabled) s.WidgetExpansions.Add(id); else s.WidgetExpansions.Remove(id);
                changed = true;
            }
            ImGui.PopStyleColor();
        }

        if (changed)
            _trackingService.SaveOverlaySettings();
    }

    private static string GetDirectionArrow(float angleRad)
    {
        var a = ((angleRad % (2 * MathF.PI)) + 2 * MathF.PI) % (2 * MathF.PI);
        return (a / (MathF.PI / 4)) switch
        {
            < 1 => "N",
            < 3 => "E",
            < 5 => "S",
            < 7 => "W",
            _ => "N",
        };
    }
}
