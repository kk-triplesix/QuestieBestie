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
    private int _filterMode = 1;
    private int _levelMin;
    private int _levelMax = 100;
    private int _classJobFilter, _locationFilter, _categoryFilter, _expansionFilter;
    private string[] _classJobOptions = [], _locationOptions = [], _categoryOptions = [], _expansionOptions = [];
    private string _classJobSearch = string.Empty, _locationSearch = string.Empty, _expansionSearch = string.Empty;
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
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(750, 500), MaximumSize = new Vector2(9999, 9999) };

        _classJobOptions = ["All Classes", .. questService.BlueQuests.Select(q => q.RequiredClassJob).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c)];
        _locationOptions = ["All Locations", .. questService.BlueQuests.Select(q => q.Location).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().OrderBy(l => l)];
        _expansionOptions = ["All Expansions", .. questService.BlueQuests.Select(q => q.Expansion).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().OrderBy(e => questService.BlueQuests.First(q => q.Expansion == e).ExpansionId)];
        _categoryOptions = ["All Types", "Feature", "Job Unlock", "Dungeon", "Trial", "Raid", "Other"];
    }

    public override void PreDraw() => Styles.PushMainStyle();
    public override void PostDraw() => Styles.PopMainStyle();

    public override void Draw()
    {
        _questService.RefreshCompletionStatus();
        DrawHeader();
        ImGui.Spacing();

        if (ImGui.BeginTabBar("##mainTabs"))
        {
            if (ImGui.BeginTabItem("Quests"))
            { ImGui.Spacing(); DrawFilterBar(); ImGui.Spacing(); DrawQuestTable(); ImGui.Spacing(); DrawStatusBar(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Aether Currents"))
            { ImGui.Spacing(); DrawAetherCurrents(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Duty Unlocks"))
            { ImGui.Spacing(); DrawDutyUnlocks(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Statistics"))
            { ImGui.Spacing(); DrawStatistics(); ImGui.EndTabItem(); }
            ImGui.EndTabBar();
        }
    }

    // ── Header ──────────────────────────────────────────────────────────

    private void DrawHeader()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("QuestieBestie"); ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("— Blue Quest Tracker"); ImGui.PopStyleColor();

        var settingsW = ImGui.CalcTextSize("Settings").X + 16;
        var overlayLabel = _overlayWindow.IsOpen ? "Hide Overlay" : "Show Overlay";
        var overlayW = ImGui.CalcTextSize(overlayLabel).X + 16;
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - settingsW - overlayW - 24);
        if (ImGui.Button(overlayLabel)) _overlayWindow.Toggle();
        ImGui.SameLine();
        if (ImGui.Button("Settings")) _settingsWindow.Toggle();

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("List:"); ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushItemWidth(160);
        var activeIdx = _trackingService.ActiveListIndex;
        var listNames = _trackingService.Lists.Select(l => l.Name).ToArray();
        if (ImGui.Combo("##listselect", ref activeIdx, listNames, listNames.Length))
            _trackingService.ActiveListIndex = activeIdx;
        ImGui.PopItemWidth();
        ImGui.SameLine();
        if (ImGui.Button("Export")) ImGui.SetClipboardText(_trackingService.ExportList());
        if (ImGui.IsItemHovered()) { ImGui.BeginTooltip(); ImGui.Text("Copy list to clipboard"); ImGui.EndTooltip(); }
        ImGui.SameLine();
        if (ImGui.Button("Import")) { var c = ImGui.GetClipboardText(); if (!string.IsNullOrWhiteSpace(c)) _trackingService.ImportList(c); }
        if (ImGui.IsItemHovered()) { ImGui.BeginTooltip(); ImGui.Text("Import list from clipboard"); ImGui.EndTooltip(); }
        ImGui.Separator();
    }

    // ── Filter Bar ──────────────────────────────────────────────────────

    private void DrawFilterBar()
    {
        ImGui.PushItemWidth(180);
        if (ImGui.InputTextWithHint("##search", "Search quests...", ref _searchText, 256)) _dirty = true;
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushItemWidth(110);
        var filterLabels = new[] { "All", "Available", "Incomplete", "Complete" };
        if (ImGui.Combo("##filter", ref _filterMode, filterLabels, filterLabels.Length)) _dirty = true;
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushItemWidth(50);
        if (ImGui.InputInt("##lvlMin", ref _levelMin, 0, 0)) { _levelMin = Math.Clamp(_levelMin, 0, 100); _dirty = true; }
        ImGui.PopItemWidth();
        ImGui.SameLine(); ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("-"); ImGui.PopStyleColor(); ImGui.SameLine();
        ImGui.PushItemWidth(50);
        if (ImGui.InputInt("##lvlMax", ref _levelMax, 0, 0)) { _levelMax = Math.Clamp(_levelMax, 0, 100); _dirty = true; }
        ImGui.PopItemWidth();
        ImGui.SameLine(); ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("Lv."); ImGui.PopStyleColor();
        ImGui.SameLine(); ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 6);
        if (ImGui.Button("Nearest")) _filtered = [.. _filtered.OrderBy(q => _questService.GetDistanceToPlayer(q))];
        if (ImGui.IsItemHovered()) { ImGui.BeginTooltip(); ImGui.Text("Sort by distance to player"); ImGui.EndTooltip(); }

        if (DrawSearchableCombo("##expansion", ref _expansionFilter, _expansionOptions, ref _expansionSearch, 130)) _dirty = true;
        ImGui.SameLine();
        if (DrawSearchableCombo("##classjob", ref _classJobFilter, _classJobOptions, ref _classJobSearch, 130)) _dirty = true;
        ImGui.SameLine();
        if (DrawSearchableCombo("##location", ref _locationFilter, _locationOptions, ref _locationSearch, 130)) _dirty = true;
        ImGui.SameLine();
        ImGui.PushItemWidth(110);
        if (ImGui.Combo("##category", ref _categoryFilter, _categoryOptions, _categoryOptions.Length)) _dirty = true;
        ImGui.PopItemWidth();
    }

    private static bool DrawSearchableCombo(string id, ref int selected, string[] options, ref string search, float width)
    {
        var changed = false;
        ImGui.PushItemWidth(width);
        if (ImGui.BeginCombo(id, options[selected]))
        {
            ImGui.PushItemWidth(width - 16);
            ImGui.InputTextWithHint($"{id}S", "Search...", ref search, 128);
            ImGui.PopItemWidth();
            var f = search.Trim();
            for (var i = 0; i < options.Length; i++)
            {
                if (f.Length > 0 && i > 0 && !options[i].Contains(f, StringComparison.OrdinalIgnoreCase)) continue;
                if (ImGui.Selectable(options[i], i == selected)) { selected = i; changed = true; search = string.Empty; }
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

        if (!ImGui.BeginTable("##quests", 7, flags, new Vector2(0, tableHeight)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("\u2605", ImGuiTableColumnFlags.WidthFixed, 24);
        ImGui.TableSetupColumn("Quest", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.DefaultSort, 0);
        ImGui.TableSetupColumn("Lv.", ImGuiTableColumnFlags.WidthFixed, 28);
        ImGui.TableSetupColumn("Exp.", ImGuiTableColumnFlags.WidthFixed, 50);
        ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("Class/Job", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("Unlocks", ImGuiTableColumnFlags.WidthFixed, 170);
        ImGui.TableHeadersRow();
        HandleSorting();

        var maxRowId = _trackingService.LastKnownMaxRowId;

        foreach (var quest in _filtered)
        {
            ImGui.TableNextRow();

            // Favorite star
            ImGui.TableNextColumn();
            var isFav = _trackingService.IsFavorite(quest.RowId);
            ImGui.PushStyleColor(ImGuiCol.Text, isFav ? Styles.FavoriteStar : Styles.TextDimmed);
            if (ImGui.Selectable($"{(isFav ? "\u2605" : "\u2606")}###fav{quest.RowId}", false, ImGuiSelectableFlags.None, new Vector2(20, 0)))
                _trackingService.ToggleFavorite(quest.RowId);
            ImGui.PopStyleColor();

            // Name with status icon, expansion color tag, "NEW" badge
            ImGui.TableNextColumn();
            var statusIcon = quest.IsCompleted ? "\u2713 " : "";
            var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;

            if (quest.IsCompleted)
            { ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextGreen); ImGui.Text("\u2713"); ImGui.PopStyleColor(); ImGui.SameLine(); }

            // "NEW" badge for quests added after last known max
            if (maxRowId > 0 && quest.RowId > maxRowId && !quest.IsCompleted)
            { ImGui.PushStyleColor(ImGuiCol.Text, Styles.FavoriteStar); ImGui.Text("NEW"); ImGui.PopStyleColor(); ImGui.SameLine(); }

            ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
            if (ImGui.Selectable($"{quest.Name}###{quest.RowId}", false, ImGuiSelectableFlags.SpanAllColumns))
            { _questService.OpenQuestOnMap(quest.RowId); _detailWindow.ShowQuest(quest); }
            ImGui.PopStyleColor();

            // Tooltip
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text(quest.Name); ImGui.PopStyleColor();
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.GetExpansionColor(quest.ExpansionId));
                ImGui.Text($"Expansion:  {quest.Expansion}");
                ImGui.PopStyleColor();
                ImGui.Text($"Level:      {quest.RequiredLevel}");
                ImGui.Text($"Location:   {quest.Location}");
                ImGui.Text($"Class/Job:  {quest.RequiredClassJob}");
                ImGui.Text($"Type:       {quest.Category}");
                if (!string.IsNullOrEmpty(quest.Unlocks)) ImGui.Text($"Unlocks:    {quest.Unlocks}");
                if (quest.PrerequisiteIds.Length > 0) ImGui.Text($"Prereqs:    {quest.PrerequisiteIds.Length}");
                if (!string.IsNullOrEmpty(quest.ChainName)) ImGui.Text($"Chain:      {quest.ChainName} ({quest.ChainIndex})");
                var note = _trackingService.GetNote(quest.RowId);
                if (!string.IsNullOrEmpty(note)) { ImGui.PushStyleColor(ImGuiCol.Text, Styles.FavoriteStar); ImGui.Text($"Note:       {note}"); ImGui.PopStyleColor(); }
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text("Click: map + details | Right-click: more"); ImGui.PopStyleColor();
                ImGui.EndTooltip();
            }

            // Context menu
            if (ImGui.BeginPopupContextItem($"ctx###{quest.RowId}"))
            { DrawQuestContextMenu(quest); ImGui.EndPopup(); }

            // Remaining columns
            var dim = quest.IsCompleted ? Styles.TextDimmed : Styles.TextSecondary;
            ImGui.TableNextColumn(); ImGui.PushStyleColor(ImGuiCol.Text, dim); ImGui.Text($"{quest.RequiredLevel}"); ImGui.PopStyleColor();

            // Expansion tag with color
            ImGui.TableNextColumn();
            var expAbbrev = quest.ExpansionId switch { 0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?" };
            var expColor = quest.IsCompleted ? Styles.TextDimmed : Styles.GetExpansionColor(quest.ExpansionId);
            ImGui.PushStyleColor(ImGuiCol.Text, expColor); ImGui.Text(expAbbrev); ImGui.PopStyleColor();

            ImGui.TableNextColumn(); ImGui.PushStyleColor(ImGuiCol.Text, dim); ImGui.Text(quest.Location); ImGui.PopStyleColor();
            ImGui.TableNextColumn(); ImGui.PushStyleColor(ImGuiCol.Text, dim); ImGui.Text(quest.RequiredClassJob); ImGui.PopStyleColor();

            ImGui.TableNextColumn();
            if (!string.IsNullOrEmpty(quest.Unlocks))
            { ImGui.PushStyleColor(ImGuiCol.Text, quest.IsCompleted ? Styles.TextDimmed : Styles.AccentCyan); ImGui.Text(quest.Unlocks); ImGui.PopStyleColor(); }
        }

        ImGui.EndTable();
    }

    // ── Context Menu ────────────────────────────────────────────────────

    private void DrawQuestContextMenu(QuestData quest)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text(quest.Name); ImGui.PopStyleColor();
        ImGui.Separator();

        // Favorite toggle
        var isFav = _trackingService.IsFavorite(quest.RowId);
        if (ImGui.MenuItem(isFav ? "\u2605 Remove Favorite" : "\u2606 Add Favorite"))
            _trackingService.ToggleFavorite(quest.RowId);

        // Map
        if (ImGui.MenuItem("Show on Map"))
            _questService.OpenQuestOnMap(quest.RowId);

        ImGui.Separator();

        // Tracking lists
        for (var i = 0; i < _trackingService.Lists.Count; i++)
        {
            var list = _trackingService.Lists[i];
            var tracked = _trackingService.IsTracked(quest.RowId, i);
            if (tracked) { if (ImGui.MenuItem($"\u2713 {list.Name}")) _trackingService.RemoveQuest(quest.RowId, i); }
            else { if (ImGui.MenuItem($"Add to {list.Name}")) _trackingService.AddQuest(quest.RowId, i); }
        }

        ImGui.Separator();
        ImGui.PushItemWidth(140);
        ImGui.InputTextWithHint("##newlist", "New list name...", ref _newListName, 64);
        ImGui.PopItemWidth();
        ImGui.SameLine();
        var ok = _newListName.Trim().Length > 0;
        if (!ok) ImGui.BeginDisabled();
        if (ImGui.Button("Create"))
        { _trackingService.CreateList(_newListName); _trackingService.AddQuest(quest.RowId, _trackingService.Lists.Count - 1); _newListName = string.Empty; ImGui.CloseCurrentPopup(); }
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
            0 => asc ? [.. _filtered.OrderByDescending(q => _trackingService.IsFavorite(q.RowId))] : [.. _filtered.OrderBy(q => _trackingService.IsFavorite(q.RowId))],
            1 => asc ? [.. _filtered.OrderBy(q => q.Name)] : [.. _filtered.OrderByDescending(q => q.Name)],
            2 => asc ? [.. _filtered.OrderBy(q => q.RequiredLevel)] : [.. _filtered.OrderByDescending(q => q.RequiredLevel)],
            3 => asc ? [.. _filtered.OrderBy(q => q.ExpansionId)] : [.. _filtered.OrderByDescending(q => q.ExpansionId)],
            4 => asc ? [.. _filtered.OrderBy(q => q.Location)] : [.. _filtered.OrderByDescending(q => q.Location)],
            5 => asc ? [.. _filtered.OrderBy(q => q.RequiredClassJob)] : [.. _filtered.OrderByDescending(q => q.RequiredClassJob)],
            6 => asc ? [.. _filtered.OrderBy(q => q.Unlocks)] : [.. _filtered.OrderByDescending(q => q.Unlocks)],
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
            switch (_filterMode) { case 1: if (q.IsCompleted || !_questService.ArePrerequisitesMet(q)) return false; break; case 2: if (q.IsCompleted) return false; break; case 3: if (!q.IsCompleted) return false; break; }
            if (_classJobFilter > 0 && !q.RequiredClassJob.Equals(_classJobOptions[_classJobFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (_locationFilter > 0 && !q.Location.Equals(_locationOptions[_locationFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (_expansionFilter > 0 && !q.Expansion.Equals(_expansionOptions[_expansionFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (_categoryFilter > 0) { QuestCategory? t = _categoryFilter switch { 1 => QuestCategory.Feature, 2 => QuestCategory.JobUnlock, 3 => QuestCategory.Dungeon, 4 => QuestCategory.Trial, 5 => QuestCategory.Raid, 6 => QuestCategory.Other, _ => null }; if (t != null && q.Category != t) return false; }
            if (q.RequiredLevel < _levelMin || q.RequiredLevel > _levelMax) return false;
            if (search.Length > 0 && !q.Name.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.RequiredClassJob.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.Location.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.Expansion.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.Unlocks.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.RequiredLevel.ToString().Contains(search, StringComparison.Ordinal)) return false;
            return true;
        }).OrderByDescending(q => _trackingService.IsFavorite(q.RowId)).ThenBy(q => q.RequiredLevel).ThenBy(q => q.Name).ToList();
    }

    // ── Status Bar ──────────────────────────────────────────────────────

    private void DrawStatusBar()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text($"{_filtered.Count}/{_questService.TotalCount} shown"); ImGui.PopStyleColor();
        ImGui.SameLine();
        var text = $"{_questService.CompletedCount}/{_questService.TotalCount} complete ({_questService.CompletionPercent:F1}%)";
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X - 20);
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentGreen); ImGui.Text(text); ImGui.PopStyleColor();
    }

    // ── Aether Currents Tab ─────────────────────────────────────────────

    private void DrawAetherCurrents()
    {
        var aetherQuests = _questService.BlueQuests
            .Where(q => q.Unlocks.Contains("Aether Current", StringComparison.OrdinalIgnoreCase))
            .GroupBy(q => q.Location)
            .OrderBy(g => g.First().ExpansionId)
            .ThenBy(g => g.Key)
            .ToList();

        if (aetherQuests.Count == 0)
        { ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed); ImGui.Text("No Aether Current quests found."); ImGui.PopStyleColor(); return; }

        foreach (var group in aetherQuests)
        {
            var total = group.Count();
            var done = group.Count(q => q.IsCompleted);
            var fraction = total > 0 ? (float)done / total : 0f;
            var expId = group.First().ExpansionId;

            ImGui.PushStyleColor(ImGuiCol.Text, Styles.GetExpansionColor(expId));
            ImGui.Text(group.Key);
            ImGui.PopStyleColor();
            ImGui.SameLine(); ImGui.SetCursorPosX(200);
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Styles.GetExpansionColor(expId));
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles.BgLight);
            ImGui.ProgressBar(fraction, new Vector2(200, 16), "");
            ImGui.PopStyleColor(2);
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text($"{done}/{total}"); ImGui.PopStyleColor();

            // Show individual quests
            foreach (var quest in group.OrderBy(q => q.RequiredLevel))
            {
                var icon = quest.IsCompleted ? "\u2713" : "\u2022";
                var color = quest.IsCompleted ? Styles.TextGreen : Styles.TextSecondary;
                ImGui.PushStyleColor(ImGuiCol.Text, color); ImGui.Text($"    {icon}"); ImGui.PopStyleColor();
                ImGui.SameLine();
                var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
                ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
                if (ImGui.Selectable($"{quest.Name} (Lv.{quest.RequiredLevel})###ac{quest.RowId}", false))
                    _questService.OpenQuestOnMap(quest.RowId);
                ImGui.PopStyleColor();
            }
            ImGui.Spacing();
        }
    }

    // ── Duty Unlocks Tab ────────────────────────────────────────────────

    private void DrawDutyUnlocks()
    {
        var dutyQuests = _questService.BlueQuests
            .Where(q => q.Category is QuestCategory.Dungeon or QuestCategory.Trial or QuestCategory.Raid)
            .OrderBy(q => q.ExpansionId)
            .ThenBy(q => q.RequiredLevel)
            .ToList();

        if (dutyQuests.Count == 0)
        { ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed); ImGui.Text("No duty unlock quests found."); ImGui.PopStyleColor(); return; }

        var categories = new[] { QuestCategory.Dungeon, QuestCategory.Trial, QuestCategory.Raid };
        foreach (var cat in categories)
        {
            var quests = dutyQuests.Where(q => q.Category == cat).ToList();
            if (quests.Count == 0) continue;

            var done = quests.Count(q => q.IsCompleted);
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
            ImGui.Text($"{cat}s ({done}/{quests.Count})");
            ImGui.PopStyleColor();
            ImGui.Separator();

            foreach (var quest in quests)
            {
                var icon = quest.IsCompleted ? "\u2713" : "\u2022";
                var iconColor = quest.IsCompleted ? Styles.TextGreen : Styles.TextSecondary;
                ImGui.PushStyleColor(ImGuiCol.Text, iconColor); ImGui.Text($"  {icon}"); ImGui.PopStyleColor();
                ImGui.SameLine();

                var expColor = quest.IsCompleted ? Styles.TextDimmed : Styles.GetExpansionColor(quest.ExpansionId);
                var expAbbrev = quest.ExpansionId switch { 0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?" };
                ImGui.PushStyleColor(ImGuiCol.Text, expColor); ImGui.Text($"[{expAbbrev}]"); ImGui.PopStyleColor();
                ImGui.SameLine();

                var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
                ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
                if (ImGui.Selectable($"Lv.{quest.RequiredLevel} {quest.Name}###du{quest.RowId}", false))
                    _questService.OpenQuestOnMap(quest.RowId);
                ImGui.PopStyleColor();

                if (!string.IsNullOrEmpty(quest.Unlocks))
                { ImGui.SameLine(); ImGui.PushStyleColor(ImGuiCol.Text, quest.IsCompleted ? Styles.TextDimmed : Styles.AccentCyan); ImGui.Text($"-> {quest.Unlocks}"); ImGui.PopStyleColor(); }
            }
            ImGui.Spacing();
        }
    }

    // ── Statistics Tab ──────────────────────────────────────────────────

    private void DrawStatistics()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("Overall Progress"); ImGui.PopStyleColor();
        ImGui.Spacing();
        DrawProgressBar("Total", _questService.CompletedCount, _questService.TotalCount, Styles.AccentGreen);
        ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("By Expansion"); ImGui.PopStyleColor();
        ImGui.Spacing();
        foreach (var stat in _questService.GetExpansionStats())
        {
            var expId = _questService.BlueQuests.FirstOrDefault(q => q.Expansion == stat.Name)?.ExpansionId ?? 0u;
            DrawProgressBar(stat.Name, stat.Completed, stat.Total, Styles.GetExpansionColor(expId));
        }
        ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("By Type"); ImGui.PopStyleColor();
        ImGui.Spacing();
        foreach (var stat in _questService.GetCategoryStats())
            DrawProgressBar(stat.Name, stat.Completed, stat.Total, Styles.AccentGreen);
    }

    private static void DrawProgressBar(string label, int completed, int total, Vector4 color)
    {
        var fraction = total > 0 ? (float)completed / total : 0f;
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextPrimary); ImGui.Text(label); ImGui.PopStyleColor();
        ImGui.SameLine(); ImGui.SetCursorPosX(200);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, color);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles.BgLight);
        ImGui.ProgressBar(fraction, new Vector2(250, 18), "");
        ImGui.PopStyleColor(2);
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text($"{completed}/{total} ({fraction * 100f:F0}%)"); ImGui.PopStyleColor();
    }
}
