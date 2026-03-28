using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class DetailWindow : Window
{
    private readonly QuestService _questService;
    private TrackingService? _trackingService;

    private QuestData? _quest;
    private List<PrerequisiteNode> _prereqTree = [];
    private string _noteText = string.Empty;

    public DetailWindow(QuestService questService)
        : base("Quest Details###QuestieBestieDetail", ImGuiWindowFlags.None)
    {
        _questService = questService;
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(380, 320), MaximumSize = new Vector2(650, 900) };
        IsOpen = false;
    }

    public void SetTrackingService(TrackingService ts) => _trackingService = ts;

    public void ShowQuest(QuestData quest)
    {
        _quest = quest;
        _prereqTree = _questService.GetPrerequisiteTree(quest.RowId);
        _noteText = _trackingService?.GetNote(quest.RowId) ?? string.Empty;
        IsOpen = true;
    }

    public override void PreDraw() => Styles.PushMainStyle();
    public override void PostDraw() => Styles.PopMainStyle();

    public override void Draw()
    {
        if (_quest == null) { ImGui.Text("No quest selected."); return; }
        _questService.RefreshCompletionStatus();

        DrawQuestHeader();
        ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
        DrawQuestInfo();
        ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
        DrawNotes();
        ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
        DrawPrerequisiteTree();
    }

    private void DrawQuestHeader()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text(_quest!.Name); ImGui.PopStyleColor();
        ImGui.SameLine();
        if (_quest.IsCompleted)
        { ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextGreen); ImGui.Text("\u2713 Complete"); ImGui.PopStyleColor(); }
        else
        { ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("\u2022 Incomplete"); ImGui.PopStyleColor(); }

        if (ImGui.Button("Show on Map"))
            _questService.OpenQuestOnMap(_quest.RowId);

        // Favorite toggle
        if (_trackingService != null)
        {
            ImGui.SameLine();
            var isFav = _trackingService.IsFavorite(_quest.RowId);
            ImGui.PushStyleColor(ImGuiCol.Text, isFav ? Styles.FavoriteStar : Styles.TextDimmed);
            if (ImGui.Button(isFav ? "\u2605 Favorited" : "\u2606 Favorite"))
                _trackingService.ToggleFavorite(_quest.RowId);
            ImGui.PopStyleColor();
        }
    }

    private void DrawQuestInfo()
    {
        var q = _quest!;
        DrawInfoLine("Expansion", q.Expansion, Styles.GetExpansionColor(q.ExpansionId));
        DrawInfoLine("Level", $"{q.RequiredLevel}");
        DrawInfoLine("Location", q.Location);
        DrawInfoLine("Class/Job", q.RequiredClassJob);
        DrawInfoLine("Type", q.Category.ToString());
        if (!string.IsNullOrEmpty(q.Unlocks))
            DrawInfoLine("Unlocks", q.Unlocks, Styles.AccentCyan);
        if (!string.IsNullOrEmpty(q.ChainName))
            DrawInfoLine("Chain", $"{q.ChainName} (Step {q.ChainIndex})");
    }

    private static void DrawInfoLine(string label, string value, Vector4? valueColor = null)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text($"{label}:"); ImGui.PopStyleColor();
        ImGui.SameLine(); ImGui.SetCursorPosX(100);
        if (valueColor.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, valueColor.Value);
        ImGui.Text(value);
        if (valueColor.HasValue) ImGui.PopStyleColor();
    }

    private void DrawNotes()
    {
        if (_trackingService == null) return;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("Notes"); ImGui.PopStyleColor();
        ImGui.Spacing();
        ImGui.PushItemWidth(-1);
        if (ImGui.InputTextMultiline("##note", ref _noteText, 512, new Vector2(0, 60)))
            _trackingService.SetNote(_quest!.RowId, _noteText);
        ImGui.PopItemWidth();
    }

    private void DrawPrerequisiteTree()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("Prerequisites"); ImGui.PopStyleColor();
        ImGui.Spacing();

        if (_prereqTree.Count == 0)
        { ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed); ImGui.Text("No prerequisites."); ImGui.PopStyleColor(); return; }

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

        ImGui.PushStyleColor(ImGuiCol.Text, iconColor); ImGui.Text($"{prefix}{icon}"); ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
        if (ImGui.Selectable($"{node.Name}{typeTag}###{node.RowId}", false, ImGuiSelectableFlags.None, new Vector2(0, 0)))
            _questService.OpenQuestOnMap(node.RowId);
        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered()) { ImGui.BeginTooltip(); ImGui.Text("Click to show on map"); ImGui.EndTooltip(); }

        foreach (var child in node.Children)
            DrawNode(child, indent + 1);
    }
}
