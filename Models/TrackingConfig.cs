using System.Numerics;
using Dalamud.Configuration;

namespace QuestieBestie.Models;

[Serializable]
public sealed class TrackingConfig : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public List<TrackingList> Lists { get; set; } = [];
    public int ActiveListIndex { get; set; }
    public OverlaySettings Overlay { get; set; } = new();
    public HashSet<uint> Favorites { get; set; } = [];
    public Dictionary<uint, string> Notes { get; set; } = [];
    public uint LastKnownMaxRowId { get; set; }
}

[Serializable]
public sealed class TrackingList
{
    public string Name { get; set; } = string.Empty;
    public List<uint> QuestRowIds { get; set; } = [];
}

[Serializable]
public sealed class OverlaySettings
{
    public float FontScale = 1.0f;
    public float BackgroundAlpha = 0.85f;
    public float BorderAlpha = 0.3f;
    public float WindowRounding = 10.0f;
    public Vector4 TextColor = new(0.92f, 0.93f, 0.95f, 1.00f);
    public Vector4 HeaderColor = new(0.33f, 0.79f, 0.79f, 1.00f);
    public Vector4 CompletedColor = new(0.31f, 0.80f, 0.64f, 1.00f);
    public Vector4 LevelColor = new(0.33f, 0.79f, 0.79f, 1.00f);
    public Vector4 WarningColor = new(0.90f, 0.35f, 0.40f, 1.00f);
    public Vector4 BackgroundColor = new(0.08f, 0.08f, 0.14f, 1.00f);
    public Vector4 BorderColor = new(0.33f, 0.79f, 0.79f, 1.00f);
    public bool AutoRemoveCompleted;
    public bool ChatNotifications = true;
    public bool SoundNotifications;
}
