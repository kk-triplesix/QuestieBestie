using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class WidgetWindow : Window
{
    private readonly QuestService _questService;
    private readonly TrackingService _trackingService;

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
    }

    public override void PreDraw()
    {
        var s = _trackingService.OverlaySettings;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, s.WindowRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 6));
        var bg = new Vector4(s.BackgroundColor.X, s.BackgroundColor.Y, s.BackgroundColor.Z, s.BackgroundAlpha);
        var border = new Vector4(s.BorderColor.X, s.BorderColor.Y, s.BorderColor.Z, s.BorderAlpha);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, bg);
        ImGui.PushStyleColor(ImGuiCol.Border, border);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(2);
        ImGui.PopStyleVar(2);
    }

    public override void Draw()
    {
        _questService.RefreshCompletionStatus();
        var s = _trackingService.OverlaySettings;

        var completed = _questService.CompletedCount;
        var total = _questService.TotalCount;
        var fraction = total > 0 ? (float)completed / total : 0f;

        ImGui.PushStyleColor(ImGuiCol.Text, s.HeaderColor);
        ImGui.Text("QB");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, s.CompletedColor);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles.BgLight);
        ImGui.ProgressBar(fraction, new Vector2(100, 14), "");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, s.TextColor);
        ImGui.Text($"{fraction * 100f:F0}%");
        ImGui.PopStyleColor();

        // Direction arrow to nearest tracked quest
        var activeList = _trackingService.ActiveList;
        var direction = _questService.GetNearestTrackedQuestDirection(activeList.QuestRowIds);
        if (direction.HasValue)
        {
            var (angle, dist, name) = direction.Value;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 4);

            // Unicode arrow based on direction
            var arrow = GetDirectionArrow(angle);
            ImGui.PushStyleColor(ImGuiCol.Text, s.LevelColor);
            ImGui.Text($"{arrow} {dist:F0}y");
            ImGui.PopStyleColor();

            if (ImGui.IsItemHovered())
            { ImGui.BeginTooltip(); ImGui.Text($"Nearest: {name} ({dist:F0} yalms)"); ImGui.EndTooltip(); }
        }
    }

    private static string GetDirectionArrow(float angleRad)
    {
        // Normalize to 0-2pi
        var a = ((angleRad % (2 * MathF.PI)) + 2 * MathF.PI) % (2 * MathF.PI);
        return (a / (MathF.PI / 4)) switch
        {
            < 1 => "\u2191",   // N
            < 3 => "\u2192",   // E
            < 5 => "\u2193",   // S
            < 7 => "\u2190",   // W
            _ => "\u2191",     // N
        };
    }
}
