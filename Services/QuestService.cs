using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using QuestieBestie.Models;

namespace QuestieBestie.Services;

public sealed class QuestService
{
    private static readonly HashSet<uint> BlueQuestIcons =
        [71201, 71221, 71241, 71261, 71281, 71301, 71321, 71341];

    public List<QuestData> BlueQuests { get; } = [];
    public Dictionary<uint, QuestData> BlueQuestLookup { get; } = [];

    private DateTime _lastRefresh = DateTime.MinValue;

    public QuestService()
    {
        LoadBlueQuests();
    }

    private void LoadBlueQuests()
    {
        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (questSheet == null)
            return;

        foreach (var quest in questSheet)
        {
            if (!BlueQuestIcons.Contains(quest.Icon))
                continue;

            var name = quest.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var prereqs = new List<uint>();
            if (quest.PreviousQuest[0].RowId != 0) prereqs.Add(quest.PreviousQuest[0].RowId);
            if (quest.PreviousQuest[1].RowId != 0) prereqs.Add(quest.PreviousQuest[1].RowId);
            if (quest.PreviousQuest[2].RowId != 0) prereqs.Add(quest.PreviousQuest[2].RowId);

            var classJob = quest.ClassJobCategory0.ValueNullable?.Name.ExtractText() ?? "All Classes";

            var questData = new QuestData
            {
                RowId = quest.RowId,
                QuestId = (ushort)(quest.RowId & 0xFFFF),
                Name = name,
                IconId = (ushort)quest.Icon,
                RequiredLevel = (byte)quest.ClassJobLevel[0],
                RequiredClassJob = classJob,
                PrerequisiteIds = prereqs.ToArray(),
            };

            BlueQuests.Add(questData);
            BlueQuestLookup[quest.RowId] = questData;
        }
    }

    public void RefreshCompletionStatus()
    {
        if ((DateTime.Now - _lastRefresh).TotalSeconds < 1.0)
            return;

        _lastRefresh = DateTime.Now;

        unsafe
        {
            var qm = QuestManager.Instance();
            if (qm == null)
                return;

            foreach (var quest in BlueQuests)
                quest.IsCompleted = QuestManager.IsQuestComplete(quest.QuestId);
        }
    }

    public (string Name, bool IsCompleted, bool IsBlueQuest) GetPrerequisiteInfo(uint rowId)
    {
        if (BlueQuestLookup.TryGetValue(rowId, out var blueQuest))
            return (blueQuest.Name, blueQuest.IsCompleted, true);

        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (questSheet == null)
            return ("Unknown", false, false);

        var quest = questSheet.GetRowOrDefault(rowId);
        if (quest == null)
            return ("Unknown", false, false);

        var name = quest.Value.Name.ExtractText();
        if (string.IsNullOrWhiteSpace(name))
            return ("Unknown", false, false);

        bool isCompleted;
        unsafe
        {
            var qm = QuestManager.Instance();
            isCompleted = qm != null && QuestManager.IsQuestComplete((ushort)(rowId & 0xFFFF));
        }

        return (name, isCompleted, false);
    }

    public int CompletedCount => BlueQuests.Count(q => q.IsCompleted);
    public int TotalCount => BlueQuests.Count;
    public float CompletionPercent => TotalCount > 0 ? (float)CompletedCount / TotalCount * 100f : 0f;
}
