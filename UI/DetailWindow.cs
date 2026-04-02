using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class DetailWindow : Window
{
    private readonly QuestService _questService;
    private readonly TrackingService _trackingService;

    private QuestData? _quest;
    private List<PrerequisiteNode> _prereqTree = [];
    private string _noteText = string.Empty;

    public DetailWindow(QuestService questService, TrackingService trackingService)
        : base("Quest Details###QuestieBestieDetail", ImGuiWindowFlags.None)
    {
        _questService = questService;
        _trackingService = trackingService;
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(380, 320) * ImGuiHelpers.GlobalScale, MaximumSize = new Vector2(650, 900) * ImGuiHelpers.GlobalScale };
        IsOpen = false;
        AllowClickthrough = false;
        AllowPinning = false;
    }

    public override void OnClose()
    {
        _quest = null;
    }

    public void ShowQuest(QuestData? quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.Name))
            return;

        _quest = quest;
        _prereqTree = _questService.GetPrerequisiteTree(quest.RowId);
        _noteText = _trackingService.GetNote(quest.RowId);
        IsOpen = true;
    }

    public override void PreDraw() => Styles.PushMainStyle();
    public override void PostDraw() => Styles.PopMainStyle();

    public override void Draw()
    {
        if (_quest == null) { IsOpen = false; return; }
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
        { Icons.DrawIcon(FontAwesomeIcon.CheckCircle, Styles.TextGreen); ImGui.SameLine(); ImGui.Text(Loc.Get("detail.complete")); }
        else
        { Icons.DrawIcon(FontAwesomeIcon.TimesCircle, Styles.TextSecondary); ImGui.SameLine(); ImGui.Text(Loc.Get("detail.incomplete")); }

        if (ImGui.Button("Show on Map"))
            _questService.OpenQuestOnMap(_quest.RowId);

        // Favorite toggle
        ImGui.SameLine();
        var isFav = _trackingService.IsFavorite(_quest.RowId);
        ImGui.PushStyleColor(ImGuiCol.Text, isFav ? Styles.FavoriteStar : Styles.TextDimmed);
        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Star, isFav ? "Favorited" : "Favorite"))
            _trackingService.ToggleFavorite(_quest.RowId);
        ImGui.PopStyleColor();
    }

    private void DrawQuestInfo()
    {
        var q = _quest!;
        DrawInfoLine(Loc.Get("detail.expansion"), q.Expansion, Styles.GetExpansionColor(q.ExpansionId));
        DrawInfoLine(Loc.Get("detail.level"), $"{q.RequiredLevel}");
        DrawInfoLine(Loc.Get("detail.location"), q.Location);
        if (!string.IsNullOrEmpty(q.NpcName))
            DrawInfoLine(Loc.Get("detail.npc"), q.NpcName);
        DrawInfoLine(Loc.Get("detail.classjob"), q.RequiredClassJob);
        DrawInfoLine(Loc.Get("detail.type"), $"{q.Category}");
        if (!string.IsNullOrEmpty(q.Unlocks))
            DrawInfoLine(Loc.Get("detail.unlocks"), q.Unlocks, Styles.AccentCyan);
        if (!string.IsNullOrEmpty(q.ChainName) && _questService.ChainLookup.TryGetValue(q.ChainName, out var chainQuests))
        {
            var chainDone = chainQuests.Count(bq => bq.IsCompleted);
            DrawInfoLine(Loc.Get("detail.chain"), $"{q.ChainName} ({Loc.Get("misc.step")} {q.ChainIndex}, {chainDone}/{chainQuests.Count})");
        }
        if (q.RewardGil > 0 || q.RewardExp > 0)
        {
            var rewards = $"{(q.RewardGil > 0 ? $"{q.RewardGil} Gil" : "")} {(q.RewardExp > 0 ? $"{q.RewardExp} EXP" : "")}".Trim();
            DrawInfoLine(Loc.Get("detail.rewards"), rewards);
        }

        // MSQ requirement indicator
        if (_prereqTree.Count > 0)
        {
            var msqPrereq = FindFirstMsq(_prereqTree);
            if (msqPrereq != null)
            {
                var msqStatus = msqPrereq.IsCompleted ? "v" : "x";
                DrawInfoLine("MSQ Req.", $"{msqStatus} {msqPrereq.Name}", msqPrereq.IsCompleted ? Styles.TextGreen : Styles.TextRed);
            }
        }
    }

    private static PrerequisiteNode? FindFirstMsq(List<PrerequisiteNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.IsMsq) return node;
            var child = FindFirstMsq(node.Children);
            if (child != null) return child;
        }
        return null;
    }

    private static void DrawInfoLine(string label, string value, Vector4? valueColor = null)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text($"{label}:"); ImGui.PopStyleColor();
        ImGui.SameLine(); ImGui.SetCursorPosX(100 * ImGuiHelpers.GlobalScale);
        if (valueColor.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, valueColor.Value);
        ImGui.Text(value);
        if (valueColor.HasValue) ImGui.PopStyleColor();
    }

    private void DrawNotes()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("Notes"); ImGui.PopStyleColor();
        ImGui.Spacing();
        ImGui.PushItemWidth(-1);
        if (ImGui.InputTextMultiline("##note", ref _noteText, 512, new Vector2(0, 60 * ImGuiHelpers.GlobalScale)))
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
        if (indent > 0) { ImGui.Text(new string(' ', indent * 3)); ImGui.SameLine(); }
        Icons.DrawCheck(node.IsCompleted);
        var nameColor = node.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
        var typeTag = node.IsMsq ? " (MSQ)" : node.IsBlueQuest ? "" : " (Side)";
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
        if (ImGui.Selectable($"{node.Name}{typeTag}###{node.RowId}", false, ImGuiSelectableFlags.None, new Vector2(0, 0)))
        {
            _questService.OpenQuestOnMap(node.RowId);
            if (node.IsBlueQuest && _questService.BlueQuestLookup.TryGetValue(node.RowId, out var prereqQuest))
                ShowQuest(prereqQuest);
        }
        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered()) { using var tt = ImRaii.Tooltip(); if (tt.Success) ImGui.Text("Click to show on map"); }

        foreach (var child in node.Children)
            DrawNode(child, indent + 1);
    }
}
