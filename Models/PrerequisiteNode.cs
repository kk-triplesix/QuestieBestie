namespace QuestieBestie.Models;

public sealed class PrerequisiteNode
{
    public uint RowId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public bool IsBlueQuest { get; init; }
    public List<PrerequisiteNode> Children { get; init; } = [];
}
