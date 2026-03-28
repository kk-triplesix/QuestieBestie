using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
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
    private readonly WidgetWindow _widgetWindow;

    private string _searchText = string.Empty;
    private int _filterMode = 1;
    private int _levelMin;
    private int _levelMax = 100;
    private int _classJobFilter, _locationFilter, _categoryFilter, _expansionFilter, _unlockFilter;
    private string[] _classJobOptions = [], _locationOptions = [], _categoryOptions = [], _expansionOptions = [], _unlockFilterOptions = [];
    private string _classJobSearch = string.Empty, _locationSearch = string.Empty, _expansionSearch = string.Empty, _unlockSearch = string.Empty;
    private List<QuestData> _filtered = [];
    private bool _dirty = true;
    private string _newListName = string.Empty;
    private readonly HashSet<uint> _selected = [];
    private DateTime _lastConfetti = DateTime.MinValue;

    public MainWindow(QuestService questService, DetailWindow detailWindow, TrackingService trackingService, OverlayWindow overlayWindow, SettingsWindow settingsWindow, WidgetWindow widgetWindow)
        : base("QuestieBestie###QuestieBestieMain", ImGuiWindowFlags.None)
    {
        _questService = questService;
        _detailWindow = detailWindow;
        _trackingService = trackingService;
        _overlayWindow = overlayWindow;
        _settingsWindow = settingsWindow;
        _widgetWindow = widgetWindow;
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(750, 500), MaximumSize = new Vector2(9999, 9999) };

        _classJobOptions = ["All Classes", .. questService.BlueQuests.Select(q => q.RequiredClassJob).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c)];
        _locationOptions = ["All Locations", .. questService.BlueQuests.Select(q => q.Location).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().OrderBy(l => l)];
        _expansionOptions = ["All Expansions", .. questService.BlueQuests.Select(q => q.Expansion).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().OrderBy(e => questService.BlueQuests.First(q => q.Expansion == e).ExpansionId)];
        _categoryOptions = ["All Types", "Feature", "Job Unlock", "Dungeon", "Trial", "Raid", "Other"];
        _unlockFilterOptions = ["All Unlocks", .. questService.BlueQuests
            .Select(q => q.Unlocks).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().OrderBy(u => u)];
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
            if (ImGui.BeginTabItem(Loc.Get("tab.quests")))
            { ImGui.Spacing(); DrawFilterBar(); ImGui.Spacing(); DrawQuestTable(); ImGui.Spacing(); DrawStatusBar(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Loc.Get("tab.aether")))
            { ImGui.Spacing(); DrawAetherCurrents(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Loc.Get("tab.duties")))
            { ImGui.Spacing(); DrawDutyUnlocks(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Loc.Get("tab.side")))
            { ImGui.Spacing(); DrawSideQuests(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Recent"))
            { ImGui.Spacing(); DrawRecent(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Loc.Get("tab.stats")))
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

        // Right-aligned buttons
        ImGui.SameLine();
        var rightX = ImGui.GetWindowWidth() - 16;
        var settingsW = ImGui.CalcTextSize("Settings").X + 16;
        var widgetLabel = _widgetWindow.IsOpen ? "Hide Widget" : "Widget";
        var widgetW = ImGui.CalcTextSize(widgetLabel).X + 16;
        var overlayLabel = _overlayWindow.IsOpen ? "Hide Overlay" : "Overlay";
        var overlayW = ImGui.CalcTextSize(overlayLabel).X + 16;
        ImGui.SetCursorPosX(rightX - settingsW - widgetW - overlayW - 16);
        if (ImGui.Button(overlayLabel)) _overlayWindow.Toggle();
        ImGui.SameLine();
        if (ImGui.Button(widgetLabel)) _widgetWindow.Toggle();
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
        if (ImGui.Button(Loc.Get("header.export"))) ImGui.SetClipboardText(_trackingService.ExportList());
        ImGui.SameLine();
        if (ImGui.Button(Loc.Get("header.import"))) { var c = ImGui.GetClipboardText(); if (!string.IsNullOrWhiteSpace(c)) _trackingService.ImportList(c); }

        // List management context menu
        ImGui.SameLine();
        if (ImGui.BeginPopup("##listMgmt"))
        {
            ImGui.PushItemWidth(140);
            ImGui.InputTextWithHint("##renameList", Loc.Get("ctx.rename"), ref _newListName, 64);
            ImGui.PopItemWidth();
            ImGui.SameLine();
            if (_newListName.Trim().Length > 0 && ImGui.Button(Loc.Get("ctx.rename")))
            { _trackingService.RenameList(_trackingService.ActiveListIndex, _newListName); _newListName = string.Empty; ImGui.CloseCurrentPopup(); }
            if (_trackingService.Lists.Count > 1 && ImGui.MenuItem(Loc.Get("ctx.delete")))
                _trackingService.DeleteList(_trackingService.ActiveListIndex);
            ImGui.EndPopup();
        }
        if (ImGuiComponents.IconButton("listEdit", FontAwesomeIcon.Edit)) ImGui.OpenPopup("##listMgmt");
        if (ImGui.IsItemHovered()) { ImGui.BeginTooltip(); ImGui.Text(Loc.Get("ctx.rename") + " / " + Loc.Get("ctx.delete")); ImGui.EndTooltip(); }

        // Undo button
        if (_trackingService.HasUndo)
        {
            ImGui.SameLine();
            if (ImGui.Button(Loc.Get("ctx.undo")))
                _trackingService.UndoRemove();
        }

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

        // Row 3: unlock filter
        if (DrawSearchableCombo("##unlock", ref _unlockFilter, _unlockFilterOptions, ref _unlockSearch, 250)) _dirty = true;
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

        // Multi-select bulk action bar
        if (_selected.Count > 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
            ImGui.Text($"{_selected.Count} selected");
            ImGui.PopStyleColor();
            ImGui.SameLine();
            for (var i = 0; i < _trackingService.Lists.Count; i++)
            {
                ImGui.SameLine();
                if (ImGui.Button($"{Loc.Get("ctx.addTo")} {_trackingService.Lists[i].Name}###bulk{i}"))
                { foreach (var id in _selected) _trackingService.AddQuest(id, i); _selected.Clear(); }
            }
            ImGui.SameLine();
            if (ImGui.Button("Clear")) _selected.Clear();
        }

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable
                    | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingStretchProp;
        var tableHeight = ImGui.GetContentRegionAvail().Y - 30;

        if (!ImGui.BeginTable("##quests", 7, flags, new Vector2(0, tableHeight)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("*", ImGuiTableColumnFlags.WidthFixed, 24);
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
            if (Icons.IconButton(isFav ? FontAwesomeIcon.Star : FontAwesomeIcon.Star, $"fav{quest.RowId}",
                isFav ? Styles.FavoriteStar : Styles.TextDimmed))
                _trackingService.ToggleFavorite(quest.RowId);

            // Name with status icon, expansion color tag, "NEW" badge
            ImGui.TableNextColumn();
            var statusIcon = quest.IsCompleted ? "\u2713 " : "";
            var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;

            // Chain indentation
            var isChainChild = !string.IsNullOrEmpty(quest.ChainName) && quest.ChainIndex > 1;
            if (isChainChild)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 16);
                Icons.DrawIcon(FontAwesomeIcon.LongArrowAltRight, Styles.TextDimmed);
                ImGui.SameLine();
            }

            if (quest.IsCompleted)
            { Icons.DrawIcon(FontAwesomeIcon.Check, Styles.TextGreen); ImGui.SameLine(); }

            // "NEW" badge
            if (maxRowId > 0 && quest.RowId > maxRowId && !quest.IsCompleted)
            { ImGui.PushStyleColor(ImGuiCol.Text, Styles.FavoriteStar); ImGui.Text("NEW"); ImGui.PopStyleColor(); ImGui.SameLine(); }

            // Category icon
            Icons.DrawIcon(Icons.GetCategoryIcon(quest.Category), quest.IsCompleted ? Styles.TextDimmed : Styles.TextSecondary);
            ImGui.SameLine();

            var isSelected = _selected.Contains(quest.RowId);
            ImGui.PushStyleColor(ImGuiCol.Text, isSelected ? Styles.AccentCyan : nameColor);
            if (ImGui.Selectable($"{quest.Name}###{quest.RowId}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
            {
                if (ImGui.GetIO().KeyCtrl)
                { if (!_selected.Remove(quest.RowId)) _selected.Add(quest.RowId); }
                else
                { _selected.Clear(); _questService.OpenQuestOnMap(quest.RowId); _detailWindow.ShowQuest(quest); _trackingService.AddRecent(quest.RowId); }
            }
            ImGui.PopStyleColor();

            // Tooltip
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text($"{quest.Category} {quest.Name}"); ImGui.PopStyleColor();
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.GetExpansionColor(quest.ExpansionId));
                ImGui.Text($"{Loc.Get("detail.expansion")}:  {quest.Expansion}");
                ImGui.PopStyleColor();
                ImGui.Text($"{Loc.Get("detail.level")}:      {quest.RequiredLevel}");
                ImGui.Text($"{Loc.Get("detail.location")}:   {quest.Location}");
                if (!string.IsNullOrEmpty(quest.NpcName)) ImGui.Text($"{Loc.Get("detail.npc")}:  {quest.NpcName}");
                ImGui.Text($"{Loc.Get("detail.classjob")}:  {quest.RequiredClassJob}");
                ImGui.Text($"{Loc.Get("detail.type")}:       {quest.Category} {quest.Category}");
                if (!string.IsNullOrEmpty(quest.Unlocks)) ImGui.Text($"{Loc.Get("detail.unlocks")}:    {quest.Unlocks}");
                if (quest.RewardGil > 0 || quest.RewardExp > 0)
                { ImGui.Text($"{Loc.Get("detail.rewards")}:    {(quest.RewardGil > 0 ? $"{quest.RewardGil} Gil" : "")} {(quest.RewardExp > 0 ? $"{quest.RewardExp} EXP" : "")}"); }
                if (quest.PrerequisiteIds.Length > 0) ImGui.Text($"{Loc.Get("detail.prereqs")}:    {quest.PrerequisiteIds.Length}");
                if (!string.IsNullOrEmpty(quest.ChainName))
                {
                    var chainQuests = _questService.BlueQuests.Where(q => q.ChainName == quest.ChainName).ToList();
                    var chainDone = chainQuests.Count(q => q.IsCompleted);
                    ImGui.Text($"{Loc.Get("detail.chain")}:      {quest.ChainName} ({Loc.Get("misc.step")} {quest.ChainIndex}, {chainDone}/{chainQuests.Count})");
                }
                var note = _trackingService.GetNote(quest.RowId);
                if (!string.IsNullOrEmpty(note)) { ImGui.PushStyleColor(ImGuiCol.Text, Styles.FavoriteStar); ImGui.Text($"{Loc.Get("detail.notes")}:       {note}"); ImGui.PopStyleColor(); }
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary); ImGui.Text(Loc.Get("misc.clickMap")); ImGui.PopStyleColor();
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
        if (ImGui.MenuItem(isFav ? "Remove Favorite" : "Add Favorite"))
            _trackingService.ToggleFavorite(quest.RowId);

        // Map + Chat
        if (ImGui.MenuItem("Show on Map"))
        { _questService.OpenQuestOnMap(quest.RowId); _detailWindow.ShowQuest(quest); }
        if (ImGui.MenuItem("Send to Chat"))
            _questService.SendQuestChatLink(quest.RowId);

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
            if (_unlockFilter > 0 && !q.Unlocks.Equals(_unlockFilterOptions[_unlockFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (q.RequiredLevel < _levelMin || q.RequiredLevel > _levelMax) return false;
            if (search.Length > 0 && !q.Name.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.RequiredClassJob.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.Location.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.Expansion.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.Unlocks.Contains(search, StringComparison.OrdinalIgnoreCase) && !q.RequiredLevel.ToString().Contains(search, StringComparison.Ordinal)) return false;
            return true;
        }).ToList();

        // Sort: favorites first, then by level, but keep chain quests grouped together
        // Chain quests sort by their first quest's level, then by ChainIndex within the chain
        _filtered = _filtered
            .OrderByDescending(q => _trackingService.IsFavorite(q.RowId))
            .ThenBy(q => !string.IsNullOrEmpty(q.ChainName)
                ? _filtered.Where(c => c.ChainName == q.ChainName).Min(c => c.RequiredLevel)
                : q.RequiredLevel)
            .ThenBy(q => q.ChainName)
            .ThenBy(q => q.ChainIndex)
            .ThenBy(q => q.Name)
            .ToList();
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
                ImGui.Text("   "); ImGui.SameLine();
                Icons.DrawCheck(quest.IsCompleted); ImGui.SameLine();
                var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
                ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
                if (ImGui.Selectable($"{quest.Name} (Lv.{quest.RequiredLevel})###ac{quest.RowId}", false))
                { _questService.OpenQuestOnMap(quest.RowId); _detailWindow.ShowQuest(quest); }
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

        // Info box
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.TextWrapped("Shows blue quest unlocks only. MSQ dungeons, Savage (NPC dialog), and individual Extreme/Ultimate fights (Minstrel dialog) are not listed as they have no quest data.");
        ImGui.PopStyleColor();
        ImGui.Spacing();

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
                ImGui.Text(" "); ImGui.SameLine();
                Icons.DrawCheck(quest.IsCompleted); ImGui.SameLine();

                var expColor = quest.IsCompleted ? Styles.TextDimmed : Styles.GetExpansionColor(quest.ExpansionId);
                var expAbbrev = quest.ExpansionId switch { 0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?" };
                ImGui.PushStyleColor(ImGuiCol.Text, expColor); ImGui.Text($"[{expAbbrev}]"); ImGui.PopStyleColor();
                ImGui.SameLine();

                var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
                ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
                if (ImGui.Selectable($"Lv.{quest.RequiredLevel} {quest.Name}###du{quest.RowId}", false))
                { _questService.OpenQuestOnMap(quest.RowId); _detailWindow.ShowQuest(quest); }
                ImGui.PopStyleColor();

                if (!string.IsNullOrEmpty(quest.Unlocks))
                { ImGui.SameLine(); ImGui.PushStyleColor(ImGuiCol.Text, quest.IsCompleted ? Styles.TextDimmed : Styles.AccentCyan); ImGui.Text($"-> {quest.Unlocks}"); ImGui.PopStyleColor(); }
            }
            ImGui.Spacing();
        }
    }

    // ── Statistics Tab ──────────────────────────────────────────────────

    // ── Side Quests Tab ───────────────────────────────────────────────

    private string _sideSearch = string.Empty;
    private int _sideFilter; // 0=All, 1=Special Only, 2=Incomplete, 3=Complete
    private int _sideSpecialFilter;
    private int _sideLocationFilter;
    private string[] _sideSpecialOptions = [];
    private string[] _sideLocationOptions = [];
    private string _sideSpecialSearch = string.Empty;
    private string _sideLocationSearch = string.Empty;
    private int _sideExpansion;

    // ── Recent Quests Tab ──────────────────────────────────────────────

    private void DrawRecent()
    {
        var recent = _trackingService.RecentQuests;
        if (recent.Count == 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextDimmed);
            ImGui.Text("No recently viewed quests.");
            ImGui.PopStyleColor();
            return;
        }

        foreach (var rowId in recent)
        {
            QuestData? quest = null;
            if (_questService.BlueQuestLookup.TryGetValue(rowId, out var bq))
                quest = bq;
            else
                quest = _questService.SideQuests.FirstOrDefault(q => q.RowId == rowId);

            if (quest == null) continue;

            Icons.DrawCheck(quest.IsCompleted); ImGui.SameLine();

            var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
            ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
            if (ImGui.Selectable($"{quest.Name} (Lv.{quest.RequiredLevel})###rec{quest.RowId}", false))
            { _questService.OpenQuestOnMap(quest.RowId); _detailWindow.ShowQuest(quest); }
            ImGui.PopStyleColor();

            ImGui.SameLine();
            var expAbbrev = quest.ExpansionId switch { 0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?" };
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.GetExpansionColor(quest.ExpansionId));
            ImGui.Text($"[{expAbbrev}]");
            ImGui.PopStyleColor();

            if (!string.IsNullOrEmpty(quest.Unlocks))
            { ImGui.SameLine(); ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text($"-> {quest.Unlocks}"); ImGui.PopStyleColor(); }
        }
    }

    // ── Side Quests Tab ─────────────────────────────────────────────────

    private void DrawSideQuests()
    {
        // Filters
        ImGui.PushItemWidth(180);
        ImGui.InputTextWithHint("##sideSearch", "Search side quests...", ref _sideSearch, 256);
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushItemWidth(120);
        var sideLabels = new[] { "All", "Special Only", "Incomplete", "Complete" };
        ImGui.Combo("##sideFilter", ref _sideFilter, sideLabels, sideLabels.Length);
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushItemWidth(140);
        ImGui.Combo("##sideExp", ref _sideExpansion, _expansionOptions, _expansionOptions.Length);
        ImGui.PopItemWidth();

        // Location + Special tag filters
        if (_sideLocationOptions.Length == 0)
        {
            _sideLocationOptions = ["All Locations", .. _questService.SideQuests
                .Select(q => q.Location).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().OrderBy(l => l)];
        }
        if (_sideSpecialOptions.Length == 0)
        {
            _sideSpecialOptions = ["All Specials", .. _questService.SideQuests
                .Where(q => q.IsSpecial && !string.IsNullOrWhiteSpace(q.SpecialTag))
                .Select(q => q.SpecialTag).Distinct().OrderBy(s => s)];
        }
        DrawSearchableCombo("##sideLocation", ref _sideLocationFilter, _sideLocationOptions, ref _sideLocationSearch, 150);
        ImGui.SameLine();
        DrawSearchableCombo("##sideSpecial", ref _sideSpecialFilter, _sideSpecialOptions, ref _sideSpecialSearch, 220);

        // Filter
        var search = _sideSearch.Trim();
        var filtered = _questService.SideQuests.Where(q =>
        {
            switch (_sideFilter)
            {
                case 1: if (!q.IsSpecial) return false; break;
                case 2: if (q.IsCompleted) return false; break;
                case 3: if (!q.IsCompleted) return false; break;
            }
            if (_sideExpansion > 0 && !q.Expansion.Equals(_expansionOptions[_sideExpansion], StringComparison.OrdinalIgnoreCase)) return false;
            if (_sideLocationFilter > 0 && !q.Location.Equals(_sideLocationOptions[_sideLocationFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (_sideSpecialFilter > 0 && !q.SpecialTag.Equals(_sideSpecialOptions[_sideSpecialFilter], StringComparison.OrdinalIgnoreCase)) return false;
            if (search.Length > 0 && !q.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !q.Location.Contains(search, StringComparison.OrdinalIgnoreCase)
                && !q.SpecialTag.Contains(search, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }).OrderByDescending(q => q.IsSpecial).ThenBy(q => q.RequiredLevel).ThenBy(q => q.Name).ToList();

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text($"{filtered.Count} quests shown ({_questService.SideQuests.Count(q => q.IsSpecial)} special)");
        ImGui.PopStyleColor();

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable
                    | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;
        var tableHeight = ImGui.GetContentRegionAvail().Y;

        if (!ImGui.BeginTable("##sidequests", 5, flags, new Vector2(0, tableHeight)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Done", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Quest", ImGuiTableColumnFlags.WidthStretch, 0);
        ImGui.TableSetupColumn("Lv.", ImGuiTableColumnFlags.WidthFixed, 28);
        ImGui.TableSetupColumn("Exp.", ImGuiTableColumnFlags.WidthFixed, 45);
        ImGui.TableSetupColumn("Special", ImGuiTableColumnFlags.WidthFixed, 250);
        ImGui.TableHeadersRow();

        foreach (var quest in filtered)
        {
            ImGui.TableNextRow();

            // Status
            ImGui.TableNextColumn();
            Icons.DrawCheck(quest.IsCompleted);

            // Name
            ImGui.TableNextColumn();
            var nameColor = quest.IsCompleted ? Styles.TextDimmed : quest.IsSpecial ? Styles.FavoriteStar : Styles.TextPrimary;
            ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
            if (ImGui.Selectable($"{quest.Name}###sq{quest.RowId}", false, ImGuiSelectableFlags.SpanAllColumns))
            { _questService.OpenQuestOnMap(quest.RowId); _detailWindow.ShowQuest(quest); }
            ImGui.PopStyleColor();

            if (ImGui.IsItemHovered() && !string.IsNullOrEmpty(quest.SpecialTag))
            { ImGui.BeginTooltip(); ImGui.Text(quest.SpecialTag); ImGui.EndTooltip(); }

            // Level
            ImGui.TableNextColumn();
            ImGui.PushStyleColor(ImGuiCol.Text, quest.IsCompleted ? Styles.TextDimmed : Styles.TextSecondary);
            ImGui.Text($"{quest.RequiredLevel}");
            ImGui.PopStyleColor();

            // Expansion
            ImGui.TableNextColumn();
            var expAbbrev = quest.ExpansionId switch { 0 => "ARR", 1 => "HW", 2 => "SB", 3 => "ShB", 4 => "EW", 5 => "DT", _ => "?" };
            ImGui.PushStyleColor(ImGuiCol.Text, quest.IsCompleted ? Styles.TextDimmed : Styles.GetExpansionColor(quest.ExpansionId));
            ImGui.Text(expAbbrev);
            ImGui.PopStyleColor();

            // Special tag
            ImGui.TableNextColumn();
            if (quest.IsSpecial)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, quest.IsCompleted ? Styles.TextDimmed : Styles.FavoriteStar);
                ImGui.Text(quest.SpecialTag);
                ImGui.PopStyleColor();
            }
        }

        ImGui.EndTable();
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

        // Quest chains progress
        var chains = _questService.BlueQuests
            .Where(q => !string.IsNullOrEmpty(q.ChainName))
            .GroupBy(q => q.ChainName)
            .OrderBy(g => g.First().ExpansionId)
            .ThenBy(g => g.First().RequiredLevel)
            .ToList();

        if (chains.Count > 0)
        {
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan); ImGui.Text("Quest Chains"); ImGui.PopStyleColor();
            ImGui.Spacing();

            foreach (var chain in chains)
            {
                var total = chain.Count();
                var done = chain.Count(q => q.IsCompleted);
                var expId = chain.First().ExpansionId;
                DrawProgressBar(chain.Key, done, total, Styles.GetExpansionColor(expId));
            }
        }
    }

    private void DrawProgressBar(string label, int completed, int total, Vector4 color)
    {
        var fraction = total > 0 ? (float)completed / total : 0f;
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextPrimary); ImGui.Text(label); ImGui.PopStyleColor();
        ImGui.SameLine(); ImGui.SetCursorPosX(200);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, color);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles.BgLight);
        ImGui.ProgressBar(fraction, new Vector2(250, 18), "");
        ImGui.PopStyleColor(2);
        ImGui.SameLine();
        if (completed == total && total > 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.FavoriteStar);
            ImGui.Text($">> {completed}/{total} (100%)");
            ImGui.PopStyleColor();

            if ((DateTime.Now - _lastConfetti).TotalSeconds > 30)
            {
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.FavoriteStar);
                ImGui.Text(">>> COMPLETE! <<<");
                ImGui.PopStyleColor();
            }
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text($"{completed}/{total} ({fraction * 100f:F0}%)");
            ImGui.PopStyleColor();
        }
    }
}
