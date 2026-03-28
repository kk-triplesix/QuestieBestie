using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class MainWindow : Window
{
    private readonly QuestService _questService;
    private readonly DetailWindow _detailWindow;
    private readonly TrackingService _trackingService;
    private readonly OverlayWindow _overlayWindow;
    private readonly SettingsWindow _settingsWindow;

    private string _searchText = string.Empty;
    private int _filterMode = 1; // 0=All, 1=Available, 2=Incomplete, 3=Complete
    private int _levelMin;
    private int _levelMax = 100;
    private int _classJobFilter;
    private int _locationFilter;
    private int _categoryFilter;
    private int _expansionFilter;
    private string[] _classJobOptions = [];
    private string[] _locationOptions = [];
    private string[] _categoryOptions = [];
    private string[] _expansionOptions = [];
    private string _classJobSearch = string.Empty;
    private string _locationSearch = string.Empty;
    private string _expansionSearch = string.Empty;
    private List<QuestData> _filtered = [];
    private bool _dirty = true;
    private string _newListName = string.Empty;

    public MainWindow(QuestService questService, DetailWindow detailWindow, TrackingService trackingService, OverlayWindow overlayWindow, SettingsWindow settingsWindow)
        : base("QuestieBestie###QuestieBestieMain", ImGuiWindowFlags.None)
    {
        _questService = questService;
        _detailWindow = detailWindow;
        _trackingService = trackingService;
        _overlayWindow = overlayWindow;
        _settingsWindow = settingsWindow;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(700, 480),
            MaximumSize = new Vector2(9999, 9999),
        };

        _classJobOptions = ["All Classes", .. questService.BlueQuests
            .Select(q => q.RequiredClassJob).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c)];
        _locationOptions = ["All Locations", .. questService.BlueQuests
            .Select(q => q.Location).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().OrderBy(l => l)];
        _expansionOptions = ["All Expansions", .. questService.BlueQuests
            .Select(q => q.Expansion).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct()
            .OrderBy(e => questService.BlueQuests.First(q => q.Expansion == e).ExpansionId)];
        _categoryOptions = ["All Types", "Feature", "Job Unlock", "Dungeon", "Trial", "Raid", "Other"];
    }

    public override void PreDraw() => Styles.PushMainStyle();
    public override void PostDraw() => Styles.PopMainStyle();

    public override void Draw()
    {
        _questService.RefreshCompletionStatus();
        DrawHeader();
        ImGui.Spacing();

        // Tabs
        if (ImGui.BeginTabBar("##mainTabs"))
        {
            if (ImGui.BeginTabItem("Quests"))
            {
                // Quests tab
                ImGui.Spacing();
                DrawFilterBar();
                ImGui.Spacing();
                DrawQuestTable();
                ImGui.Spacing();
                DrawStatusBar();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Statistics"))
            {
                // Stats tab
                ImGui.Spacing();
                DrawStatistics();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }

    // ── Header ──────────────────────────────────────────────────────────

    private void DrawHeader()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("QuestieBestie");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("— Blue Quest Tracker");
        ImGui.PopStyleColor();

        var settingsW = ImGui.CalcTextSize("Settings").X + 16;
        var overlayLabel = _overlayWindow.IsOpen ? "Hide Overlay" : "Show Overlay";
        var overlayW = ImGui.CalcTextSize(overlayLabel).X + 16;
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - settingsW - overlayW - 24);
        if (ImGui.Button(overlayLabel)) _overlayWindow.Toggle();
        ImGui.SameLine();
        if (ImGui.Button("Settings")) _settingsWindow.Toggle();

        // List selector
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("List:");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushItemWidth(160);
        var activeIdx = _trackingService.ActiveListIndex;
        var listNames = _trackingService.Lists.Select(l => l.Name).ToArray();
        if (ImGui.Combo("##listselect", ref activeIdx, listNames, listNames.Length))
            _trackingService.ActiveListIndex = activeIdx;
        ImGui.PopItemWidth();

        ImGui.Separator();
    }

    // ── Filter Bar ──────────────────────────────────────────────────────

    private void DrawFilterBar()
    {
        // Row 1: search, status, level
        ImGui.PushItemWidth(180);
        if (ImGui.InputTextWithHint("##search", "Search quests...", ref _searchText, 256))
            _dirty = true;
        ImGui.PopItemWidth();

        ImGui.SameLine();
        ImGui.PushItemWidth(110);
        var filterLabels = new[] { "All", "Available", "Incomplete", "Complete" };
        if (ImGui.Combo("##filter", ref _filterMode, filterLabels, filterLabels.Length))
            _dirty = true;
        ImGui.PopItemWidth();

        ImGui.SameLine();
        ImGui.PushItemWidth(50);
        if (ImGui.InputInt("##lvlMin", ref _levelMin, 0, 0)) { _levelMin = Math.Clamp(_levelMin, 0, 100); _dirty = true; }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("-"); ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushItemWidth(50);
        if (ImGui.InputInt("##lvlMax", ref _levelMax, 0, 0)) { _levelMax = Math.Clamp(_levelMax, 0, 100); _dirty = true; }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("Lv."); ImGui.PopStyleColor();

        // Row 2: expansion, class/job, location, category
        if (DrawSearchableCombo("##expansion", ref _expansionFilter, _expansionOptions, ref _expansionSearch, 140))
            _dirty = true;
        ImGui.SameLine();
        if (DrawSearchableCombo("##classjob", ref _classJobFilter, _classJobOptions, ref _classJobSearch, 140))
            _dirty = true;
        ImGui.SameLine();
        if (DrawSearchableCombo("##location", ref _locationFilter, _locationOptions, ref _locationSearch, 140))
            _dirty = true;
        ImGui.SameLine();
        ImGui.PushItemWidth(110);
        if (ImGui.Combo("##category", ref _categoryFilter, _categoryOptions, _categoryOptions.Length))
            _dirty = true;
        ImGui.PopItemWidth();
    }

    private static bool DrawSearchableCombo(string id, ref int selected, string[] options, ref string search, float width)
    {
        var changed = false;
        ImGui.PushItemWidth(width);
        if (ImGui.BeginCombo(id, options[selected]))
        {
            ImGui.PushItemWidth(width - 16);
            ImGui.InputTextWithHint($"{id}Search", "Search...", ref search, 128);
            ImGui.PopItemWidth();
            var filter = search.Trim();
            for (var i = 0; i < options.Length; i++)
            {
                if (filter.Length > 0 && i > 0 && !options[i].Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (ImGui.Selectable(options[i], i == selected))
                { selected = i; changed = true; search = string.Empty; }
            }
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();
        return changed;
    }

    // ── Quest Table ─────────────────────────────────────────────────────

    private void DrawQuestTable()
    {
        if (_dirty) { ApplyFilters(); _dirty = false; }

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable
                    | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingStretchProp;
        var tableHeight = ImGui.GetContentRegionAvail().Y - 30;

        if (!ImGui.BeginTable("##quests", 6, flags, new Vector2(0, tableHeight)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Done", ImGuiTableColumnFlags.WidthFixed, 36);
        ImGui.TableSetupColumn("Quest", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.DefaultSort, 0);
        ImGui.TableSetupColumn("Lv.", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthFixed, 110);
        ImGui.TableSetupColumn("Class/Job", ImGuiTableColumnFlags.WidthFixed, 110);
        ImGui.TableSetupColumn("Unlocks", ImGuiTableColumnFlags.WidthFixed, 180);
        ImGui.TableHeadersRow();
        HandleSorting();

        foreach (var quest in _filtered)
        {
            ImGui.TableNextRow();

            // Status
            ImGui.TableNextColumn();
            var statusIcon = quest.IsCompleted ? " \u2713" : " \u2022";
            var statusColor = quest.IsCompleted ? Styles.TextGreen : Styles.TextSecondary;
            ImGui.PushStyleColor(ImGuiCol.Text, statusColor);
            ImGui.Text(statusIcon);
            ImGui.PopStyleColor();

            // Name (clickable)
            ImGui.TableNextColumn();
            var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
            ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
            if (ImGui.Selectable($"{quest.Name}###{quest.RowId}", false, ImGuiSelectableFlags.SpanAllColumns))
            {
                _questService.OpenQuestOnMap(quest.RowId);
                _detailWindow.ShowQuest(quest);
            }
            ImGui.PopStyleColor();

            // Tooltip
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
                ImGui.Text(quest.Name);
                ImGui.PopStyleColor();
                ImGui.Separator();
                ImGui.Text($"Expansion:  {quest.Expansion}");
                ImGui.Text($"Level:      {quest.RequiredLevel}");
                ImGui.Text($"Location:   {quest.Location}");
                ImGui.Text($"Class/Job:  {quest.RequiredClassJob}");
                ImGui.Text($"Type:       {quest.Category}");
                if (!string.IsNullOrEmpty(quest.Unlocks))
                    ImGui.Text($"Unlocks:    {quest.Unlocks}");
                if (quest.PrerequisiteIds.Length > 0)
                {
                    ImGui.Text($"Prereqs:    {quest.PrerequisiteIds.Length}");
                }
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
                ImGui.Text("Click: map + details | Right-click: track");
                ImGui.PopStyleColor();
                ImGui.EndTooltip();
            }

            // Context menu
            if (ImGui.BeginPopupContextItem($"ctx###{quest.RowId}"))
            { DrawQuestContextMenu(quest); ImGui.EndPopup(); }

            // Remaining columns
            var dim = quest.IsCompleted ? Styles.TextDimmed : Styles.TextSecondary;

            ImGui.TableNextColumn();
            ImGui.PushStyleColor(ImGuiCol.Text, dim); ImGui.Text($"{quest.RequiredLevel}"); ImGui.PopStyleColor();

            ImGui.TableNextColumn();
            ImGui.PushStyleColor(ImGuiCol.Text, dim); ImGui.Text(quest.Location); ImGui.PopStyleColor();

            ImGui.TableNextColumn();
            ImGui.PushStyleColor(ImGuiCol.Text, dim); ImGui.Text(quest.RequiredClassJob); ImGui.PopStyleColor();

            ImGui.TableNextColumn();
            if (!string.IsNullOrEmpty(quest.Unlocks))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, quest.IsCompleted ? Styles.TextDimmed : Styles.AccentCyan);
                ImGui.Text(quest.Unlocks);
                ImGui.PopStyleColor();
            }
        }

        ImGui.EndTable();
    }

    // ── Context Menu ────────────────────────────────────────────────────

    private void DrawQuestContextMenu(QuestData quest)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text(quest.Name);
        ImGui.PopStyleColor();
        ImGui.Separator();

        for (var i = 0; i < _trackingService.Lists.Count; i++)
        {
            var list = _trackingService.Lists[i];
            var tracked = _trackingService.IsTracked(quest.RowId, i);
            if (tracked)
            { if (ImGui.MenuItem($"\u2713 {list.Name}")) _trackingService.RemoveQuest(quest.RowId, i); }
            else
            { if (ImGui.MenuItem($"Add to {list.Name}")) _trackingService.AddQuest(quest.RowId, i); }
        }

        ImGui.Separator();
        ImGui.PushItemWidth(140);
        ImGui.InputTextWithHint("##newlist", "New list name...", ref _newListName, 64);
        ImGui.PopItemWidth();
        ImGui.SameLine();
        var ok = _newListName.Trim().Length > 0;
        if (!ok) ImGui.BeginDisabled();
        if (ImGui.Button("Create"))
        {
            _trackingService.CreateList(_newListName);
            _trackingService.AddQuest(quest.RowId, _trackingService.Lists.Count - 1);
            _newListName = string.Empty;
            ImGui.CloseCurrentPopup();
        }
        if (!ok) ImGui.EndDisabled();
    }

    // ── Sorting ─────────────────────────────────────────────────────────

    private void HandleSorting()
    {
        var sortSpecs = ImGui.TableGetSortSpecs();
        if (!sortSpecs.SpecsDirty) return;
        var specs = sortSpecs.Specs;
        var asc = specs.SortDirection == ImGuiSortDirection.Ascending;
        _filtered = specs.ColumnIndex switch
        {
            0 => asc ? [.. _filtered.OrderBy(q => q.IsCompleted)] : [.. _filtered.OrderByDescending(q => q.IsCompleted)],
            1 => asc ? [.. _filtered.OrderBy(q => q.Name)] : [.. _filtered.OrderByDescending(q => q.Name)],
            2 => asc ? [.. _filtered.OrderBy(q => q.RequiredLevel)] : [.. _filtered.OrderByDescending(q => q.RequiredLevel)],
            3 => asc ? [.. _filtered.OrderBy(q => q.Location)] : [.. _filtered.OrderByDescending(q => q.Location)],
            4 => asc ? [.. _filtered.OrderBy(q => q.RequiredClassJob)] : [.. _filtered.OrderByDescending(q => q.RequiredClassJob)],
            5 => asc ? [.. _filtered.OrderBy(q => q.Unlocks)] : [.. _filtered.OrderByDescending(q => q.Unlocks)],
            _ => _filtered
        };
        sortSpecs.SpecsDirty = false;
    }

    // ── Filters ─────────────────────────────────────────────────────────

    private void ApplyFilters()
    {
        var search = _searchText.Trim();
        _filtered = _questService.BlueQuests.Where(q =>
        {
            switch (_filterMode)
            {
                case 1: if (q.IsCompleted || !_questService.ArePrerequisitesMet(q)) return false; break;
                case 2: if (q.IsCompleted) return false; break;
                case 3: if (!q.IsCompleted) return false; break;
            }
            if (_classJobFilter > 0 && !q.RequiredClassJob.Equals(_classJobOptions[_classJobFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (_locationFilter > 0 && !q.Location.Equals(_locationOptions[_locationFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (_expansionFilter > 0 && !q.Expansion.Equals(_expansionOptions[_expansionFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (_categoryFilter > 0)
            {
                QuestCategory? t = _categoryFilter switch { 1 => QuestCategory.Feature, 2 => QuestCategory.JobUnlock, 3 => QuestCategory.Dungeon, 4 => QuestCategory.Trial, 5 => QuestCategory.Raid, 6 => QuestCategory.Other, _ => null };
                if (t != null && q.Category != t) return false;
            }
            if (q.RequiredLevel < _levelMin || q.RequiredLevel > _levelMax) return false;
            if (search.Length > 0
                && !q.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !q.RequiredClassJob.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !q.Location.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !q.Expansion.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !q.Unlocks.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !q.RequiredLevel.ToString().Contains(search, StringComparison.Ordinal)) return false;
            return true;
        }).OrderBy(q => q.RequiredLevel).ThenBy(q => q.Name).ToList();
    }

    // ── Status Bar ──────────────────────────────────────────────────────

    private void DrawStatusBar()
    {
        var completed = _questService.CompletedCount;
        var total = _questService.TotalCount;
        var percent = _questService.CompletionPercent;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text($"{_filtered.Count}/{total} shown");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        var text = $"{completed}/{total} complete ({percent:F1}%)";
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X - 20);
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentGreen);
        ImGui.Text(text);
        ImGui.PopStyleColor();
    }

    // ── Statistics Tab ──────────────────────────────────────────────────

    private void DrawStatistics()
    {
        var expansionStats = _questService.GetExpansionStats();
        var categoryStats = _questService.GetCategoryStats();

        // Overall progress
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("Overall Progress");
        ImGui.PopStyleColor();
        ImGui.Spacing();
        DrawProgressBar("Total", _questService.CompletedCount, _questService.TotalCount, Styles.AccentGreen);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Per expansion
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("By Expansion");
        ImGui.PopStyleColor();
        ImGui.Spacing();
        foreach (var stat in expansionStats)
            DrawProgressBar(stat.Name, stat.Completed, stat.Total, Styles.AccentCyan);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Per category
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("By Type");
        ImGui.PopStyleColor();
        ImGui.Spacing();
        foreach (var stat in categoryStats)
            DrawProgressBar(stat.Name, stat.Completed, stat.Total, Styles.AccentGreen);
    }

    private static void DrawProgressBar(string label, int completed, int total, Vector4 color)
    {
        var fraction = total > 0 ? (float)completed / total : 0f;
        var percent = fraction * 100f;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextPrimary);
        ImGui.Text($"{label}");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.SetCursorPosX(200);

        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, color);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles.BgLight);
        ImGui.ProgressBar(fraction, new Vector2(250, 18), "");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text($"{completed}/{total} ({percent:F0}%)");
        ImGui.PopStyleColor();
    }
}
