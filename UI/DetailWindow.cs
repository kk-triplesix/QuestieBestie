using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class DetailWindow : Window
{
    private readonly QuestService _questService;

    private QuestData? _quest;
    private List<PrerequisiteNode> _prereqTree = [];

    public DetailWindow(QuestService questService)
        : base("Quest Details###QuestieBestieDetail", ImGuiWindowFlags.None)
    {
        _questService = questService;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 280),
            MaximumSize = new Vector2(600, 800),
        };
        IsOpen = false;
    }

    public void ShowQuest(QuestData quest)
    {
        _quest = quest;
        _prereqTree = _questService.GetPrerequisiteTree(quest.RowId);
        IsOpen = true;
    }

    public override void PreDraw()
    {
        Styles.PushMainStyle();
    }

    public override void PostDraw()
    {
        Styles.PopMainStyle();
    }

    public override void Draw()
    {
        if (_quest == null)
        {
            ImGui.Text("No quest selected.");
            return;
        }

        _questService.RefreshCompletionStatus();

        DrawQuestHeader();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        DrawQuestInfo();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        DrawPrerequisiteTree();
    }

    private void DrawQuestHeader()
    {
        // Quest name
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text(_quest!.Name);
        ImGui.PopStyleColor();

        // Status
        ImGui.SameLine();
        if (_quest.IsCompleted)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextGreen);
            ImGui.Text("\u2713 Complete");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text("\u2022 Incomplete");
            ImGui.PopStyleColor();
        }

        // Map button
        if (ImGui.Button("Show on Map"))
            _questService.OpenQuestOnMap(_quest.RowId);
    }

    private void DrawQuestInfo()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("Level:");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.Text($"{_quest!.RequiredLevel}");

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("Class/Job:");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.Text(_quest.RequiredClassJob);
    }

    private void DrawPrerequisiteTree()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("Prerequisites");
        ImGui.PopStyleColor();

        ImGui.Spacing();

        if (_prereqTree.Count == 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed);
            ImGui.Text("No prerequisites.");
            ImGui.PopStyleColor();
            return;
        }

        foreach (var node in _prereqTree)
            DrawNode(node, 0);
    }

    private void DrawNode(PrerequisiteNode node, int indent)
    {
        var prefix = new string(' ', indent * 2);
        var icon = node.IsCompleted ? "\u2713" : "\u2717";
        var iconColor = node.IsCompleted ? Styles.TextGreen : Styles.TextRed;
        var nameColor = node.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
        var typeTag = node.IsMsq ? " (MSQ)" : node.IsBlueQuest ? "" : " (Side)";

        // Icon
        ImGui.PushStyleColor(ImGuiCol.Text, iconColor);
        ImGui.Text($"{prefix}{icon}");
        ImGui.PopStyleColor();

        // Name as clickable link
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
        if (ImGui.Selectable($"{node.Name}{typeTag}###{node.RowId}", false, ImGuiSelectableFlags.None, new Vector2(0, 0)))
            _questService.OpenQuestOnMap(node.RowId);
        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Click to show on map");
            ImGui.EndTooltip();
        }

        // Recursively draw children
        foreach (var child in node.Children)
            DrawNode(child, indent + 1);
    }
}
