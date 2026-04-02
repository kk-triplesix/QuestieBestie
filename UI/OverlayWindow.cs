using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class OverlayWindow : Window, IDisposable
{
    private readonly QuestService _questService;
    private readonly TrackingService _trackingService;
    private readonly SettingsWindow _settingsWindow;
    private readonly DetailWindow _detailWindow;

    private bool _showButtons;
    private DateTime _lastHovered = DateTime.MinValue;

    public OverlayWindow(QuestService questService, TrackingService trackingService, SettingsWindow settingsWindow, DetailWindow detailWindow)
        : base("##QuestieBestieOverlay",
            ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoBringToFrontOnFocus)
    {
        _questService = questService;
        _trackingService = trackingService;
        _settingsWindow = settingsWindow;
        _detailWindow = detailWindow;
        IsOpen = false;
        RespectCloseHotkey = false;
        AllowClickthrough = false;
        AllowPinning = false;
        ShowCloseButton = false;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 80) * ImGuiHelpers.GlobalScale,
            MaximumSize = new Vector2(800, 1200) * ImGuiHelpers.GlobalScale,
        };
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        var s = _trackingService.OverlaySettings;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, s.WindowRounding * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14, 10) * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 4) * ImGuiHelpers.GlobalScale);

        var alpha = _showButtons ? Math.Max(s.BackgroundAlpha, 0.85f) : s.BackgroundAlpha;
        var bg = new Vector4(s.BackgroundColor.X, s.BackgroundColor.Y, s.BackgroundColor.Z, alpha);
        var border = new Vector4(s.BorderColor.X, s.BorderColor.Y, s.BorderColor.Z, s.BorderAlpha);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, bg);
        ImGui.PushStyleColor(ImGuiCol.Border, border);
        ImGui.PushStyleColor(ImGuiCol.Text, s.TextColor);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, Styles.TextDimmed);

        if (Math.Abs(s.FontScale - 1.0f) > 0.01f)
            ImGui.SetWindowFontScale(s.FontScale);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(4);
        ImGui.PopStyleVar(3);
        ImGui.SetWindowFontScale(1.0f);
    }

    public override void Draw()
    {
        var s = _trackingService.OverlaySettings;
        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem))
            _lastHovered = DateTime.Now;
        _showButtons = (DateTime.Now - _lastHovered).TotalSeconds < 2.0;

        DrawHeader(s);
        ImGui.Separator();
        DrawListSwitcher(s);
        ImGui.Separator();
        DrawTrackedQuests(s);
    }

    private void DrawHeader(OverlaySettings s)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, s.HeaderColor);
        ImGui.Text("QuestieBestie");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text($"({_questService.CompletionPercent:F0}%)");
        ImGui.PopStyleColor();

        // Settings + Close buttons (right-aligned, only on hover)
        if (_showButtons)
        {
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 64 * ImGuiHelpers.GlobalScale);
            if (ImGuiComponents.IconButton("ovSettings", FontAwesomeIcon.Cog))
                _settingsWindow.Toggle();
            if (ImGui.IsItemHovered())
            { using var tt = ImRaii.Tooltip(); if (tt.Success) ImGui.Text(Loc.Get("header.settings")); }
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextRed);
            if (ImGuiComponents.IconButton("ovClose", FontAwesomeIcon.Times))
                IsOpen = false;
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
            { using var tt = ImRaii.Tooltip(); if (tt.Success) ImGui.Text(Loc.Get("misc.close")); }
        }
    }

    private void DrawListSwitcher(OverlaySettings s)
    {
        var lists = _trackingService.Lists;
        if (lists.Count <= 1)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text(_trackingService.ActiveList.Name);
            ImGui.PopStyleColor();
            return;
        }

        for (var i = 0; i < lists.Count; i++)
        {
            if (i > 0) ImGui.SameLine();

            var isActive = i == _trackingService.ActiveListIndex;
            ImGui.PushStyleColor(ImGuiCol.Text, isActive ? s.HeaderColor : Styles.TextDimmed);

            if (ImGui.Selectable($"{lists[i].Name}###list{i}", isActive, ImGuiSelectableFlags.None, new Vector2(ImGui.CalcTextSize(lists[i].Name).X + 8 * ImGuiHelpers.GlobalScale, 0)))
                _trackingService.ActiveListIndex = i;

            ImGui.PopStyleColor();
        }
    }

    private void DrawTrackedQuests(OverlaySettings s)
    {
        var activeList = _trackingService.ActiveList;

        if (activeList.QuestRowIds.Count == 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed);
            ImGui.Text(Loc.Get("overlay.noQuests"));
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text(Loc.Get("overlay.addHint"));
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            return;
        }

        // Plan Route button
        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Route, Loc.Get("misc.planRoute")))
        {
            var quests = activeList.QuestRowIds
                .Where(id => _questService.BlueQuestLookup.TryGetValue(id, out var q) && !q.IsCompleted)
                .Select(id => _questService.BlueQuestLookup[id])
                .ToList();
            var sorted = _questService.PlanRoute(quests);
            activeList.QuestRowIds.Clear();
            activeList.QuestRowIds.AddRange(sorted.Select(q => q.RowId));
            // Re-add completed ones at the end
            foreach (var id in activeList.QuestRowIds.ToList())
                if (_questService.BlueQuestLookup.TryGetValue(id, out var q) && q.IsCompleted && !activeList.QuestRowIds.Contains(id))
                    activeList.QuestRowIds.Add(id);
            _trackingService.SaveOverlaySettings();
        }
        if (ImGui.IsItemHovered())
        { using var tt = ImRaii.Tooltip(); if (tt.Success) ImGui.Text(Loc.Get("misc.sortRoute")); }

        ImGui.Separator();

        foreach (var rowId in activeList.QuestRowIds.ToList())
        {
            if (!_questService.BlueQuestLookup.TryGetValue(rowId, out var quest))
                continue;

            if (quest.IsCompleted)
            {
                Icons.DrawIcon(FontAwesomeIcon.Check, s.CompletedColor);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, s.LevelColor);
                ImGui.Text($"[{quest.RequiredLevel,2}]");
                ImGui.PopStyleColor();
            }

            ImGui.SameLine();

            var nameColor = quest.IsCompleted ? Styles.TextDimmed : s.TextColor;
            ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
            if (ImGui.Selectable($"{quest.Name}###ov{quest.RowId}", false))
                _questService.OpenQuestOnMap(quest.RowId);
            ImGui.PopStyleColor();

            using (var popup = ImRaii.ContextPopupItem($"ovctx###{quest.RowId}"))
            {
                if (popup.Success)
                {
                    var isManual = _trackingService.IsManuallyCompleted(quest.RowId);
                    if (isManual)
                    { if (ImGui.MenuItem(Loc.Get("misc.unmarkComplete"))) _trackingService.UnmarkCompleted(quest.RowId); }
                    else
                    { if (ImGui.MenuItem(Loc.Get("misc.markComplete"))) _trackingService.MarkCompleted(quest.RowId, _questService); }
                    ImGui.Separator();
                    if (ImGui.MenuItem(Loc.Get("ctx.remove")))
                        _trackingService.RemoveQuest(quest.RowId);
                }
            }

            if (!quest.IsCompleted && quest.PrerequisiteIds.Length > 0)
            {
                var missing = quest.PrerequisiteIds
                    .Select(id => _questService.GetPrerequisiteInfo(id))
                    .Where(p => !p.IsCompleted)
                    .ToList();

                if (missing.Count > 0)
                {
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, s.WarningColor);
                    ImGui.Text($"! {missing.Count} {Loc.Get("overlay.reqShort")}");
                    ImGui.PopStyleColor();

                    if (ImGui.IsItemHovered())
                    {
                        using var tt = ImRaii.Tooltip();
                        if (tt.Success)
                        {
                            ImGui.Text(Loc.Get("misc.missingReqs"));
                            foreach (var (name, _, isBlue) in missing)
                            {
                                var tag = isBlue ? "" : Loc.Get("overlay.tagMsqSide");
                                ImGui.PushStyleColor(ImGuiCol.Text, s.WarningColor);
                                ImGui.Text($"  x {name}{tag}");
                                ImGui.PopStyleColor();
                            }
                        }
                    }
                }
            }
        }
    }
}
