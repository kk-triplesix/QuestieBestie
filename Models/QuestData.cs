namespace QuestieBestie.Models;

public enum QuestCategory
{
    Feature,
    JobUnlock,
    Dungeon,
    Trial,
    Raid,
    Other,
}

public sealed class QuestData
{
    public uint RowId { get; init; }
    public ushort QuestId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ushort IconId { get; init; }
    public byte RequiredLevel { get; init; }
    public string RequiredClassJob { get; init; } = string.Empty;
    public uint[] PrerequisiteIds { get; init; } = [];
    public bool IsCompleted { get; set; }
    public bool IsClassQuest { get; init; }
    public string Location { get; init; } = string.Empty;
    public QuestCategory Category { get; init; }
    public string Unlocks { get; init; } = string.Empty;
}
