using Dalamud.Configuration;

namespace QuestieBestie.Models;

[Serializable]
public sealed class TrackingConfig : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public List<TrackingList> Lists { get; set; } = [];
    public int ActiveListIndex { get; set; }
}

[Serializable]
public sealed class TrackingList
{
    public string Name { get; set; } = string.Empty;
    public List<uint> QuestRowIds { get; set; } = [];
}
