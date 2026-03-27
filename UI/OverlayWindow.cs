using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class OverlayWindow : Window
{
    private readonly QuestService _questService;
    private readonly TrackingService _trackingService;

    public OverlayWindow(QuestService questService, TrackingService trackingService)
        : base("##QuestieBestieOverlay",
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
        Styles.PushOverlayStyle();
    }

    public override void PostDraw()
    {
        Styles.PopOverlayStyle();
    }

    public override void Draw()
    {
        _questService.RefreshCompletionStatus();

        DrawHeader();
        ImGui.Separator();
        DrawListSwitcher();
        ImGui.Separator();
        DrawTrackedQuests();
    }

    private void DrawHeader()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("QuestieBestie");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text($"({_questService.CompletionPercent:F0}%)");
        ImGui.PopStyleColor();
    }

    private void DrawListSwitcher()
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
            if (isActive)
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
            else
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed);

            if (ImGui.Selectable($"{lists[i].Name}###list{i}", isActive, ImGuiSelectableFlags.None, new Vector2(ImGui.CalcTextSize(lists[i].Name).X + 8, 0)))
                _trackingService.ActiveListIndex = i;

            ImGui.PopStyleColor();
        }
    }

    private void DrawTrackedQuests()
    {
        var activeList = _trackingService.ActiveList;

        if (activeList.QuestRowIds.Count == 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed);
            ImGui.Text("No tracked quests.");
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text("Right-click quests in main window to add.");
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            return;
        }

        foreach (var rowId in activeList.QuestRowIds.ToList())
        {
            if (!_questService.BlueQuestLookup.TryGetValue(rowId, out var quest))
                continue;

            // Status icon
            if (quest.IsCompleted)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextGreen);
                ImGui.Text("\u2713");
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
                ImGui.Text($"[{quest.RequiredLevel,2}]");
                ImGui.PopStyleColor();
            }

            ImGui.SameLine();

            // Quest name — clickable
            var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
            ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
            if (ImGui.Selectable($"{quest.Name}###ov{quest.RowId}", false))
                _questService.OpenQuestOnMap(quest.RowId);
            ImGui.PopStyleColor();

            // Right-click to remove
            if (ImGui.BeginPopupContextItem($"ovctx###{quest.RowId}"))
            {
                if (ImGui.MenuItem("Remove from list"))
                    _trackingService.RemoveQuest(quest.RowId);
                ImGui.EndPopup();
            }

            // Missing prereqs indicator
            if (!quest.IsCompleted && quest.PrerequisiteIds.Length > 0)
            {
                var missing = quest.PrerequisiteIds
                    .Select(id => _questService.GetPrerequisiteInfo(id))
                    .Where(p => !p.IsCompleted)
                    .ToList();

                if (missing.Count > 0)
                {
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextRed);
                    ImGui.Text($"\u26a0 {missing.Count} req");
                    ImGui.PopStyleColor();

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextPrimary);
                        ImGui.Text("Missing requirements:");
                        ImGui.PopStyleColor();
                        foreach (var (name, _, isBlue) in missing)
                        {
                            var tag = isBlue ? "" : " (MSQ/Side)";
                            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextRed);
                            ImGui.Text($"  \u2717 {name}{tag}");
                            ImGui.PopStyleColor();
                        }
                        ImGui.EndTooltip();
                    }
                }
            }
        }
    }
}
