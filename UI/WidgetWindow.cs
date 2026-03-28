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
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 3));
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
        var isHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows);

        // Total progress (if enabled)
        if (s.WidgetShowTotal)
            DrawBar("Total", _questService.CompletedCount, _questService.TotalCount, s.CompletedColor, s);

        // Per-expansion progress bars
        foreach (var stat in _questService.GetExpansionStats())
        {
            var expId = _questService.BlueQuests.FirstOrDefault(q => q.Expansion == stat.Name)?.ExpansionId ?? 0u;
            if (!s.WidgetExpansions.Contains(expId))
                continue;

            var abbrev = expId switch { 0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?" };
            DrawBar(abbrev, stat.Completed, stat.Total, Styles.GetExpansionColor(expId), s);
        }

        // Per-chain progress bars
        var chains = _questService.BlueQuests
            .Where(q => !string.IsNullOrEmpty(q.ChainName) && s.WidgetChains.Contains(q.ChainName))
            .GroupBy(q => q.ChainName)
            .OrderBy(g => g.First().ExpansionId);

        foreach (var chain in chains)
        {
            var total = chain.Count();
            var done = chain.Count(q => q.IsCompleted);
            var expId = chain.First().ExpansionId;
            var label = chain.Key.Length > 12 ? chain.Key[..12] + ".." : chain.Key;
            DrawBar(label, done, total, Styles.GetExpansionColor(expId), s);
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

        // Config button (always visible, small)
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
        if (ImGui.Button("S###wCfg"))
            ImGui.OpenPopup("##widgetCfg");
        ImGui.PopStyleVar();
        if (ImGui.IsItemHovered())
        { ImGui.BeginTooltip(); ImGui.Text("Settings"); ImGui.EndTooltip(); }

        DrawConfigPopup();
    }

    private static void DrawBar(string label, int completed, int total, Vector4 color, Models.OverlaySettings s)
    {
        var fraction = total > 0 ? (float)completed / total : 0f;

        ImGui.PushStyleColor(ImGuiCol.Text, s.HeaderColor);
        ImGui.Text(label);
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.SetCursorPosX(80);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, color);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles.BgLight);
        ImGui.ProgressBar(fraction, new Vector2(100, 14), "");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();
        ImGui.SetCursorPosX(190);
        ImGui.PushStyleColor(ImGuiCol.Text, s.TextColor);
        ImGui.Text($"{fraction * 100f:F0}%");
        ImGui.PopStyleColor();
    }

    private void DrawConfigPopup()
    {
        if (!ImGui.BeginPopup("##widgetCfg"))
            return;

        var s = _trackingService.OverlaySettings;
        var changed = false;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("Widget Bars");
        ImGui.PopStyleColor();
        ImGui.Separator();

        // Total toggle
        if (ImGui.Checkbox("Total", ref s.WidgetShowTotal))
            changed = true;

        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("Expansions");
        ImGui.PopStyleColor();

        // Expansion toggles
        var expansionNames = new (uint Id, string Name)[]
        { (0, "A Realm Reborn"), (1, "Heavensward"), (2, "Stormblood"), (3, "Shadowbringers"), (4, "Endwalker"), (5, "Dawntrail") };

        foreach (var (id, name) in expansionNames)
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

        // Chain toggles
        var allChains = _questService.BlueQuests
            .Where(q => !string.IsNullOrEmpty(q.ChainName))
            .Select(q => q.ChainName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        if (allChains.Count > 0)
        {
            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text("Quest Chains");
            ImGui.PopStyleColor();

            foreach (var chainName in allChains)
            {
                var enabled = s.WidgetChains.Contains(chainName);
                if (ImGui.Checkbox($"{chainName}###wCh{chainName.GetHashCode()}", ref enabled))
                {
                    if (enabled) s.WidgetChains.Add(chainName); else s.WidgetChains.Remove(chainName);
                    changed = true;
                }
            }
        }

        if (changed)
            _trackingService.SaveOverlaySettings();

        ImGui.EndPopup();
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
