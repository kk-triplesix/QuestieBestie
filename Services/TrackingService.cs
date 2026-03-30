using ECommons.DalamudServices;
using QuestieBestie.Models;

namespace QuestieBestie.Services;

public sealed class TrackingService
{
    private readonly TrackingConfig _config;

    public TrackingService()
    {
        _config = Svc.PluginInterface.GetPluginConfig() as TrackingConfig ?? new TrackingConfig();
        if (_config.Lists.Count == 0)
            _config.Lists.Add(new TrackingList { Name = "Default" });
    }

    public OverlaySettings OverlaySettings => _config.Overlay;
    public HashSet<uint> Favorites => _config.Favorites;
    public Dictionary<uint, string> Notes => _config.Notes;
    public uint LastKnownMaxRowId { get => _config.LastKnownMaxRowId; set { _config.LastKnownMaxRowId = value; Save(); } }
    public IReadOnlyList<TrackingList> Lists => _config.Lists;
    public int ActiveListIndex
    {
        get => Math.Clamp(_config.ActiveListIndex, 0, Math.Max(0, _config.Lists.Count - 1));
        set
        {
            _config.ActiveListIndex = Math.Clamp(value, 0, Math.Max(0, _config.Lists.Count - 1));
            Save();
        }
    }

    public TrackingList ActiveList => _config.Lists[ActiveListIndex];

    public void CreateList(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        _config.Lists.Add(new TrackingList { Name = name.Trim() });
        _config.ActiveListIndex = _config.Lists.Count - 1;
        Save();
    }

    public void DeleteList(int index)
    {
        if (_config.Lists.Count <= 1 || index < 0 || index >= _config.Lists.Count)
            return;

        _config.Lists.RemoveAt(index);
        if (_config.ActiveListIndex >= _config.Lists.Count)
            _config.ActiveListIndex = _config.Lists.Count - 1;
        Save();
    }

    public void RenameList(int index, string newName)
    {
        if (index < 0 || index >= _config.Lists.Count || string.IsNullOrWhiteSpace(newName))
            return;

        _config.Lists[index].Name = newName.Trim();
        Save();
    }

    public void AddQuest(uint rowId, int? listIndex = null)
    {
        var list = _config.Lists[listIndex ?? ActiveListIndex];
        if (!list.QuestRowIds.Contains(rowId))
        {
            list.QuestRowIds.Add(rowId);
            Save();
        }
    }

    private (int ListIndex, uint QuestRowId)? _lastRemoved;

    public void RemoveQuest(uint rowId, int? listIndex = null)
    {
        var idx = listIndex ?? ActiveListIndex;
        var list = _config.Lists[idx];
        if (list.QuestRowIds.Remove(rowId))
        {
            _lastRemoved = (idx, rowId);
            Save();
        }
    }

    public bool UndoRemove()
    {
        if (_lastRemoved == null) return false;
        var (idx, rowId) = _lastRemoved.Value;
        if (idx < _config.Lists.Count && !_config.Lists[idx].QuestRowIds.Contains(rowId))
        {
            _config.Lists[idx].QuestRowIds.Add(rowId);
            Save();
        }
        _lastRemoved = null;
        return true;
    }

    public bool HasUndo => _lastRemoved != null;

    private readonly List<uint> _recentQuests = [];
    public IReadOnlyList<uint> RecentQuests => _recentQuests;

    public void AddRecent(uint rowId)
    {
        _recentQuests.Remove(rowId);
        _recentQuests.Insert(0, rowId);
        if (_recentQuests.Count > 20)
            _recentQuests.RemoveAt(20);
    }

    public bool IsTracked(uint rowId, int? listIndex = null)
    {
        var list = _config.Lists[listIndex ?? ActiveListIndex];
        return list.QuestRowIds.Contains(rowId);
    }

    public bool IsTrackedInAnyList(uint rowId)
    {
        return _config.Lists.Any(l => l.QuestRowIds.Contains(rowId));
    }

    public void ToggleFavorite(uint rowId)
    {
        if (!_config.Favorites.Remove(rowId))
            _config.Favorites.Add(rowId);
        Save();
    }

    public bool IsFavorite(uint rowId) => _config.Favorites.Contains(rowId);

    public HashSet<uint> ManuallyCompleted => _config.ManuallyCompleted;

    public bool IsManuallyCompleted(uint rowId) => _config.ManuallyCompleted.Contains(rowId);

    public void MarkCompleted(uint rowId, QuestService questService)
    {
        _config.ManuallyCompleted.Add(rowId);
        MarkPrerequisitesCompleted(rowId, questService);
        Save();
    }

    public void UnmarkCompleted(uint rowId)
    {
        _config.ManuallyCompleted.Remove(rowId);
        Save();
    }

    public void ClearManualCompletions()
    {
        _config.ManuallyCompleted.Clear();
        Save();
    }

    private void MarkPrerequisitesCompleted(uint rowId, QuestService questService)
    {
        if (!questService.BlueQuestLookup.TryGetValue(rowId, out var quest))
            return;

        foreach (var prereqId in quest.PrerequisiteIds)
        {
            if (_config.ManuallyCompleted.Add(prereqId))
                MarkPrerequisitesCompleted(prereqId, questService);
        }
    }

    public void SetNote(uint rowId, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            _config.Notes.Remove(rowId);
        else
            _config.Notes[rowId] = note.Trim();
        Save();
    }

    public string GetNote(uint rowId) => _config.Notes.GetValueOrDefault(rowId, string.Empty);

    public string ExportList(int? listIndex = null)
    {
        var list = _config.Lists[listIndex ?? ActiveListIndex];
        var export = new { list.Name, list.QuestRowIds };
        return System.Text.Json.JsonSerializer.Serialize(export);
    }

    public bool ImportList(string json)
    {
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            var name = root.GetProperty("Name").GetString() ?? "Imported";
            var ids = root.GetProperty("QuestRowIds").EnumerateArray()
                .Select(e => e.GetUInt32()).ToList();

            _config.Lists.Add(new TrackingList { Name = name, QuestRowIds = ids });
            _config.ActiveListIndex = _config.Lists.Count - 1;
            Save();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<(uint RowId, bool YouCompleted, bool TheyCompleted)> CompareWithImport(string json, QuestService questService)
    {
        var result = new List<(uint, bool, bool)>();
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var ids = doc.RootElement.GetProperty("QuestRowIds").EnumerateArray()
                .Select(e => e.GetUInt32()).ToHashSet();

            foreach (var quest in questService.BlueQuests)
            {
                var youDone = quest.IsCompleted;
                var theyDone = ids.Contains(quest.RowId);
                if (youDone != theyDone)
                    result.Add((quest.RowId, youDone, theyDone));
            }
        }
        catch { }
        return result;
    }

    public void SaveOverlaySettings()
    {
        Save();
    }

    private void Save()
    {
        Svc.PluginInterface.SavePluginConfig(_config);
    }
}
