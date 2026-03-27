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
    private int _filterMode = 1; // 0 = All, 1 = Available, 2 = Incomplete, 3 = Complete
    private int _levelMin;
    private int _levelMax = 100;
    private bool _hideClassQuests = true;
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

        // Tracking lists section
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 280);
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("List:");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushItemWidth(120);
        var lists = _trackingService.Lists;
        var activeIdx = _trackingService.ActiveListIndex;
        var listNames = lists.Select(l => l.Name).ToArray();
        if (ImGui.Combo("##listselect", ref activeIdx, listNames, listNames.Length))
            _trackingService.ActiveListIndex = activeIdx;
        ImGui.PopItemWidth();

        ImGui.SameLine();
        var overlayLabel = _overlayWindow.IsOpen ? "Hide Overlay" : "Show Overlay";
        if (ImGui.Button(overlayLabel))
            _overlayWindow.Toggle();

        ImGui.SameLine();
        if (ImGui.Button("Settings"))
            _settingsWindow.Toggle();

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
        var filterLabels = new[] { "All", "Available", "Incomplete", "Complete" };
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

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
        if (ImGui.Checkbox("Hide class quests", ref _hideClassQuests))
            _dirty = true;
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
        ImGui.TableSetupColumn("Done", ImGuiTableColumnFlags.WidthFixed, 36);
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

            // Name column — clickable to open map + details
            ImGui.TableNextColumn();
            var nameColor = quest.IsCompleted ? Styles.TextDimmed : Styles.TextPrimary;
            ImGui.PushStyleColor(ImGuiCol.Text, nameColor);
            if (ImGui.Selectable($"{quest.Name}###{quest.RowId}", false, ImGuiSelectableFlags.SpanAllColumns))
            {
                _questService.OpenQuestOnMap(quest.RowId);
                _detailWindow.ShowQuest(quest);
            }
            ImGui.PopStyleColor();

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Click: Show on map + details");
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
                ImGui.Text("Right-click: Add to tracking list");
                ImGui.PopStyleColor();
                ImGui.EndTooltip();
            }

            // Right-click context menu
            if (ImGui.BeginPopupContextItem($"ctx###{quest.RowId}"))
            {
                DrawQuestContextMenu(quest);
                ImGui.EndPopup();
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

    private void DrawQuestContextMenu(QuestData quest)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text(quest.Name);
        ImGui.PopStyleColor();
        ImGui.Separator();

        // Add to existing lists
        for (var i = 0; i < _trackingService.Lists.Count; i++)
        {
            var list = _trackingService.Lists[i];
            var isTracked = _trackingService.IsTracked(quest.RowId, i);

            if (isTracked)
            {
                if (ImGui.MenuItem($"\u2713 {list.Name}"))
                    _trackingService.RemoveQuest(quest.RowId, i);
            }
            else
            {
                if (ImGui.MenuItem($"Add to {list.Name}"))
                    _trackingService.AddQuest(quest.RowId, i);
            }
        }

        ImGui.Separator();

        // Create new list
        ImGui.PushItemWidth(140);
        ImGui.InputTextWithHint("##newlist", "New list name...", ref _newListName, 64);
        ImGui.PopItemWidth();

        ImGui.SameLine();
        var canCreate = _newListName.Trim().Length > 0;
        if (!canCreate) ImGui.BeginDisabled();
        if (ImGui.Button("Create"))
        {
            _trackingService.CreateList(_newListName);
            _trackingService.AddQuest(quest.RowId, _trackingService.Lists.Count - 1);
            _newListName = string.Empty;
            ImGui.CloseCurrentPopup();
        }
        if (!canCreate) ImGui.EndDisabled();
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
            0 => ascending
                ? [.. _filtered.OrderBy(q => q.IsCompleted)]
                : [.. _filtered.OrderByDescending(q => q.IsCompleted)],
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
                // Status filter
                switch (_filterMode)
                {
                    case 1: // Available: not completed + all prereqs met
                        if (q.IsCompleted || !_questService.ArePrerequisitesMet(q)) return false;
                        break;
                    case 2: // Incomplete
                        if (q.IsCompleted) return false;
                        break;
                    case 3: // Complete
                        if (!q.IsCompleted) return false;
                        break;
                }

                // Hide class quests
                if (_hideClassQuests && !q.RequiredClassJob.Contains("All Classes", StringComparison.OrdinalIgnoreCase)) return false;

                // Level range
                if (q.RequiredLevel < _levelMin || q.RequiredLevel > _levelMax) return false;

                // Text search across all columns
                if (search.Length > 0
                    && !q.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    && !q.RequiredClassJob.Contains(search, StringComparison.OrdinalIgnoreCase)
                    && !q.RequiredLevel.ToString().Contains(search, StringComparison.Ordinal)) return false;

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
