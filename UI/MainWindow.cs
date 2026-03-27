using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class MainWindow : Window
{
    private readonly QuestService _questService;

    private string _searchText = string.Empty;
    private int _filterMode; // 0 = All, 1 = Incomplete, 2 = Complete
    private int _levelMin;
    private int _levelMax = 100;
    private List<QuestData> _filtered = [];
    private bool _dirty = true;

    public MainWindow(QuestService questService)
        : base("QuestieBestie###QuestieBestieMain", ImGuiWindowFlags.None)
    {
        _questService = questService;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(620, 420),
            MaximumSize = new Vector2(9999, 9999),
        };
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
        _questService.RefreshCompletionStatus();

        DrawHeader();
        ImGui.Spacing();
        DrawFilterBar();
        ImGui.Spacing();
        DrawQuestTable();
        ImGui.Spacing();
        DrawStatusBar();
    }

    private void DrawHeader()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("QuestieBestie");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("— Blue Quest Tracker");
        ImGui.PopStyleColor();

        ImGui.Separator();
    }

    private void DrawFilterBar()
    {
        ImGui.PushItemWidth(200);
        if (ImGui.InputTextWithHint("##search", "Search quests...", ref _searchText, 256))
            _dirty = true;
        ImGui.PopItemWidth();

        ImGui.SameLine();
        ImGui.PushItemWidth(120);
        var filterLabels = new[] { "All", "Incomplete", "Complete" };
        if (ImGui.Combo("##filter", ref _filterMode, filterLabels, filterLabels.Length))
            _dirty = true;
        ImGui.PopItemWidth();

        ImGui.SameLine();
        ImGui.PushItemWidth(60);
        if (ImGui.InputInt("##lvlMin", ref _levelMin, 0, 0))
        {
            _levelMin = Math.Clamp(_levelMin, 0, 100);
            _dirty = true;
        }
        ImGui.PopItemWidth();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("-");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushItemWidth(60);
        if (ImGui.InputInt("##lvlMax", ref _levelMax, 0, 0))
        {
            _levelMax = Math.Clamp(_levelMax, 0, 100);
            _dirty = true;
        }
        ImGui.PopItemWidth();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("Lv.");
        ImGui.PopStyleColor();
    }

    private void DrawQuestTable()
    {
        if (_dirty)
        {
            ApplyFilters();
            _dirty = false;
        }

        var flags = ImGuiTableFlags.Borders
                    | ImGuiTableFlags.RowBg
                    | ImGuiTableFlags.Resizable
                    | ImGuiTableFlags.ScrollY
                    | ImGuiTableFlags.Sortable
                    | ImGuiTableFlags.SizingStretchProp;

        var avail = ImGui.GetContentRegionAvail();
        var tableHeight = avail.Y - 30;

        if (!ImGui.BeginTable("##quests", 4, flags, new Vector2(0, tableHeight)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Done", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 36);
        ImGui.TableSetupColumn("Quest", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.DefaultSort, 0);
        ImGui.TableSetupColumn("Lv.", ImGuiTableColumnFlags.WidthFixed, 36);
        ImGui.TableSetupColumn("Class/Job", ImGuiTableColumnFlags.WidthFixed, 140);
        ImGui.TableHeadersRow();

        HandleSorting();

        foreach (var quest in _filtered)
        {
            ImGui.TableNextRow();

            // Status column
            ImGui.TableNextColumn();
            if (quest.IsCompleted)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextGreen);
                ImGui.Text(" \u2713");
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
                ImGui.Text(" \u2022");
                ImGui.PopStyleColor();
            }

            // Name column with prerequisites
            ImGui.TableNextColumn();
            if (quest.IsCompleted)
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed);

            if (quest.PrerequisiteIds.Length > 0)
            {
                var open = ImGui.TreeNode($"{quest.Name}###{quest.RowId}");

                if (quest.IsCompleted)
                    ImGui.PopStyleColor();

                if (open)
                {
                    DrawPrerequisites(quest);
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text(quest.Name);
                if (quest.IsCompleted)
                    ImGui.PopStyleColor();
            }

            // Level column
            ImGui.TableNextColumn();
            var lvlColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextSecondary;
            ImGui.PushStyleColor(ImGuiCol.Text, lvlColor);
            ImGui.Text($"{quest.RequiredLevel}");
            ImGui.PopStyleColor();

            // Class/Job column
            ImGui.TableNextColumn();
            ImGui.PushStyleColor(ImGuiCol.Text, lvlColor);
            ImGui.Text(quest.RequiredClassJob);
            ImGui.PopStyleColor();
        }

        ImGui.EndTable();
    }

    private void DrawPrerequisites(QuestData quest)
    {
        foreach (var prereqId in quest.PrerequisiteIds)
        {
            var (name, isCompleted, isBlueQuest) = _questService.GetPrerequisiteInfo(prereqId);

            var icon = isCompleted ? "\u2713" : "\u2717";
            var color = isCompleted ? Styles.TextGreen : Styles.TextRed;
            var typeTag = isBlueQuest ? "" : " (MSQ/Side)";

            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.Text($"  {icon}");
            ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, isCompleted ? Styles.TextDimmed : Styles.TextPrimary);
            ImGui.Text($"{name}{typeTag}");
            ImGui.PopStyleColor();

            // Show level/class for blue prereqs
            if (isBlueQuest && _questService.BlueQuestLookup.TryGetValue(prereqId, out var prereqQuest))
            {
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
                ImGui.Text($"(Lv.{prereqQuest.RequiredLevel})");
                ImGui.PopStyleColor();
            }
        }
    }

    private void HandleSorting()
    {
        var sortSpecs = ImGui.TableGetSortSpecs();
        if (!sortSpecs.SpecsDirty)
            return;

        var specs = sortSpecs.Specs;
        var ascending = specs.SortDirection == ImGuiSortDirection.Ascending;

        _filtered = specs.ColumnIndex switch
        {
            1 => ascending
                ? [.. _filtered.OrderBy(q => q.Name)]
                : [.. _filtered.OrderByDescending(q => q.Name)],
            2 => ascending
                ? [.. _filtered.OrderBy(q => q.RequiredLevel)]
                : [.. _filtered.OrderByDescending(q => q.RequiredLevel)],
            3 => ascending
                ? [.. _filtered.OrderBy(q => q.RequiredClassJob)]
                : [.. _filtered.OrderByDescending(q => q.RequiredClassJob)],
            _ => _filtered
        };

        sortSpecs.SpecsDirty = false;
    }

    private void ApplyFilters()
    {
        var search = _searchText.Trim();

        _filtered = _questService.BlueQuests
            .Where(q =>
            {
                if (_filterMode == 1 && q.IsCompleted) return false;
                if (_filterMode == 2 && !q.IsCompleted) return false;
                if (q.RequiredLevel < _levelMin || q.RequiredLevel > _levelMax) return false;
                if (search.Length > 0 && !q.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) return false;
                return true;
            })
            .OrderBy(q => q.RequiredLevel)
            .ThenBy(q => q.Name)
            .ToList();
    }

    private void DrawStatusBar()
    {
        var completed = _questService.CompletedCount;
        var total = _questService.TotalCount;
        var percent = _questService.CompletionPercent;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text($"{_filtered.Count}/{total} shown");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        var progressText = $"{completed}/{total} complete ({percent:F1}%)";
        var textWidth = ImGui.CalcTextSize(progressText).X;
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - textWidth - 20);
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentGreen);
        ImGui.Text(progressText);
        ImGui.PopStyleColor();
    }
}
