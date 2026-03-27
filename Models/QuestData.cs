namespace QuestieBestie.Models;

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
}
