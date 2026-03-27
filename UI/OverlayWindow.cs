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
        var s = _trackingService.OverlaySettings;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, s.WindowRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14, 10));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 4));

        var bg = new Vector4(s.BackgroundColor.X, s.BackgroundColor.Y, s.BackgroundColor.Z, s.BackgroundAlpha);
        var border = new Vector4(s.BorderColor.X, s.BorderColor.Y, s.BorderColor.Z, s.BorderAlpha);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, bg);
        ImGui.PushStyleColor(ImGuiCol.Border, border);
        ImGui.PushStyleColor(ImGuiCol.Text, s.TextColor);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, Styles.TextDimmed);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(4);
        ImGui.PopStyleVar(3);

        if (_fontScaled)
        {
            ImGui.SetWindowFontScale(1.0f);
            _fontScaled = false;
        }
    }

    private bool _fontScaled;

    public override void Draw()
    {
        var s = _trackingService.OverlaySettings;

        if (Math.Abs(s.FontScale - 1.0f) > 0.01f)
        {
            ImGui.SetWindowFontScale(s.FontScale);
            _fontScaled = true;
        }

        _questService.RefreshCompletionStatus();

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

            if (ImGui.Selectable($"{lists[i].Name}###list{i}", isActive, ImGuiSelectableFlags.None, new Vector2(ImGui.CalcTextSize(lists[i].Name).X + 8, 0)))
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

            if (quest.IsCompleted)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, s.CompletedColor);
                ImGui.Text("\u2713");
                ImGui.PopStyleColor();
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

            if (ImGui.BeginPopupContextItem($"ovctx###{quest.RowId}"))
            {
                if (ImGui.MenuItem("Remove from list"))
                    _trackingService.RemoveQuest(quest.RowId);
                ImGui.EndPopup();
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
                    ImGui.Text($"\u26a0 {missing.Count} req");
                    ImGui.PopStyleColor();

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Missing requirements:");
                        foreach (var (name, _, isBlue) in missing)
                        {
                            var tag = isBlue ? "" : " (MSQ/Side)";
                            ImGui.PushStyleColor(ImGuiCol.Text, s.WarningColor);
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
