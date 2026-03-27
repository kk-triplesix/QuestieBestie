using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class OverlayWindow : Window
{
    private readonly QuestService _questService;
    private const int MaxDisplayQuests = 15;

    public OverlayWindow(QuestService questService)
        : base("##QuestieBestieOverlay",
            ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoBringToFrontOnFocus)
    {
        _questService = questService;
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

        // Header
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("QuestieBestie");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        var percent = _questService.CompletionPercent;
        ImGui.Text($"({percent:F0}%)");
        ImGui.PopStyleColor();

        ImGui.Separator();

        // Show incomplete quests sorted by level
        var incomplete = _questService.BlueQuests
            .Where(q => !q.IsCompleted)
            .OrderBy(q => q.RequiredLevel)
            .ThenBy(q => q.Name)
            .Take(MaxDisplayQuests)
            .ToList();

        if (incomplete.Count == 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentGreen);
            ImGui.Text("All blue quests complete!");
            ImGui.PopStyleColor();
        }
        else
        {
            foreach (var quest in incomplete)
            {
                // Level badge
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
                ImGui.Text($"[{quest.RequiredLevel,2}]");
                ImGui.PopStyleColor();

                ImGui.SameLine();

                // Quest name
                ImGui.Text(quest.Name);

                // Missing prerequisites indicator
                if (quest.PrerequisiteIds.Length > 0)
                {
                    var missingPrereqs = quest.PrerequisiteIds
                        .Select(id => _questService.GetPrerequisiteInfo(id))
                        .Where(p => !p.IsCompleted)
                        .ToList();

                    if (missingPrereqs.Count > 0)
                    {
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextRed);
                        ImGui.Text($"\u26a0 {missingPrereqs.Count} req");
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextPrimary);
                            ImGui.Text("Missing requirements:");
                            ImGui.PopStyleColor();
                            foreach (var (name, _, isBlue) in missingPrereqs)
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

            var remaining = _questService.BlueQuests.Count(q => !q.IsCompleted);
            if (remaining > MaxDisplayQuests)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed);
                ImGui.Text($"... and {remaining - MaxDisplayQuests} more");
                ImGui.PopStyleColor();
            }
        }
    }
}
