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

    public void RemoveQuest(uint rowId, int? listIndex = null)
    {
        var list = _config.Lists[listIndex ?? ActiveListIndex];
        if (list.QuestRowIds.Remove(rowId))
            Save();
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

    public void SaveOverlaySettings()
    {
        Save();
    }

    private void Save()
    {
        Svc.PluginInterface.SavePluginConfig(_config);
    }
}
