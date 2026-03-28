using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using QuestieBestie.Models;

namespace QuestieBestie.Services;

public sealed class QuestService
{
    private static readonly HashSet<uint> BlueQuestEventIconTypes = [8, 10];

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
            if (!BlueQuestEventIconTypes.Contains(quest.EventIconType.RowId))
                continue;

            var name = quest.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            // Skip removed/deprecated quests (no valid quest giver location)
            if (quest.IssuerLocation.RowId == 0)
                continue;

            var prereqs = new List<uint>();
            if (quest.PreviousQuest[0].RowId != 0) prereqs.Add(quest.PreviousQuest[0].RowId);
            if (quest.PreviousQuest[1].RowId != 0) prereqs.Add(quest.PreviousQuest[1].RowId);
            if (quest.PreviousQuest[2].RowId != 0) prereqs.Add(quest.PreviousQuest[2].RowId);

            var classJob = quest.ClassJobCategory0.ValueNullable?.Name.ExtractText() ?? "All Classes";
            var isClassQuest = quest.ClassJobRequired.RowId != 0;

            // Location from quest PlaceName
            var location = quest.PlaceName.ValueNullable?.Name.ExtractText() ?? "";
            if (string.IsNullOrWhiteSpace(location))
                location = quest.IssuerLocation.ValueNullable?.Territory.ValueNullable?.PlaceName.ValueNullable?.Name.ExtractText() ?? "";

            // Determine category and unlock description
            var (category, unlocks) = DetermineQuestCategory(quest);

            var questData = new QuestData
            {
                RowId = quest.RowId,
                QuestId = (ushort)(quest.RowId & 0xFFFF),
                Name = name,
                IconId = (ushort)quest.Icon,
                RequiredLevel = (byte)quest.ClassJobLevel[0],
                RequiredClassJob = classJob,
                PrerequisiteIds = prereqs.ToArray(),
                IsClassQuest = isClassQuest,
                Location = location,
                Category = category,
                Unlocks = unlocks,
            };

            BlueQuests.Add(questData);
            BlueQuestLookup[quest.RowId] = questData;
        }

        // Remove duplicates: keep the quest with the highest RowId (newest version)
        var dupeNames = BlueQuests
            .GroupBy(q => q.Name)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.OrderByDescending(q => q.RowId).Skip(1))
            .Select(q => q.RowId)
            .ToHashSet();

        if (dupeNames.Count > 0)
        {
            BlueQuests.RemoveAll(q => dupeNames.Contains(q.RowId));
            foreach (var id in dupeNames)
                BlueQuestLookup.Remove(id);
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

    public void OpenQuestOnMap(uint rowId)
    {
        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (questSheet == null)
            return;

        var quest = questSheet.GetRowOrDefault(rowId);
        if (quest == null)
            return;

        var issuerLocation = quest.Value.IssuerLocation.ValueNullable;
        if (issuerLocation == null)
            return;

        var level = issuerLocation.Value;
        var map = level.Map.ValueNullable;
        var territory = level.Territory.ValueNullable;
        if (map == null || territory == null)
            return;

        var mapCoords = MapUtil.WorldToMap(new Vector2(level.X, level.Z), map.Value);
        var payload = new MapLinkPayload(territory.Value.RowId, map.Value.RowId, mapCoords.X, mapCoords.Y);
        Svc.GameGui.OpenMapWithMapLink(payload);
    }

    public List<PrerequisiteNode> GetPrerequisiteTree(uint rowId, int maxDepth = 10)
    {
        var visited = new HashSet<uint>();
        return BuildPrerequisiteTree(rowId, visited, maxDepth);
    }

    private List<PrerequisiteNode> BuildPrerequisiteTree(uint rowId, HashSet<uint> visited, int depth)
    {
        if (depth <= 0)
            return [];

        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (questSheet == null)
            return [];

        var quest = questSheet.GetRowOrDefault(rowId);
        if (quest == null)
            return [];

        var prereqIds = new List<uint>();
        if (quest.Value.PreviousQuest[0].RowId != 0) prereqIds.Add(quest.Value.PreviousQuest[0].RowId);
        if (quest.Value.PreviousQuest[1].RowId != 0) prereqIds.Add(quest.Value.PreviousQuest[1].RowId);
        if (quest.Value.PreviousQuest[2].RowId != 0) prereqIds.Add(quest.Value.PreviousQuest[2].RowId);

        var nodes = new List<PrerequisiteNode>();
        foreach (var prereqId in prereqIds)
        {
            if (!visited.Add(prereqId))
                continue;

            var (name, isCompleted, isBlueQuest) = GetPrerequisiteInfo(prereqId);
            if (name == "Unknown")
                continue;

            // Check if this prereq is an MSQ — show it but don't recurse deeper
            var isMsq = IsMsqQuest(prereqId);
            var children = isMsq ? [] : BuildPrerequisiteTree(prereqId, visited, depth - 1);

            nodes.Add(new PrerequisiteNode
            {
                RowId = prereqId,
                Name = name,
                IsCompleted = isCompleted,
                IsBlueQuest = isBlueQuest,
                IsMsq = isMsq,
                Children = children,
            });
        }

        return nodes;
    }

    private (QuestCategory Category, string Unlocks) DetermineQuestCategory(Quest quest)
    {
        // Job unlock
        var jobUnlock = quest.ClassJobUnlock.ValueNullable;
        if (jobUnlock != null)
        {
            var jobName = jobUnlock.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(jobName))
                return (QuestCategory.JobUnlock, $"Unlocks {jobName}");
        }

        // Instance content unlock (dungeon/trial/raid)
        if (quest.InstanceContentUnlock.RowId != 0)
        {
            var cfcSheet = Svc.Data.GetExcelSheet<ContentFinderCondition>();
            if (cfcSheet != null)
            {
                foreach (var cfc in cfcSheet)
                {
                    if (cfc.Content.RowId == quest.InstanceContentUnlock.RowId && cfc.ContentLinkType == 1)
                    {
                        var contentName = cfc.Name.ExtractText();
                        if (string.IsNullOrWhiteSpace(contentName))
                            break;

                        var contentTypeId = cfc.ContentType.RowId;
                        return contentTypeId switch
                        {
                            2 => (QuestCategory.Dungeon, $"Unlocks {contentName}"),
                            4 => (QuestCategory.Trial, $"Unlocks {contentName}"),
                            5 or 28 or 37 => (QuestCategory.Raid, $"Unlocks {contentName}"),
                            _ => (QuestCategory.Other, $"Unlocks {contentName}"),
                        };
                    }
                }
            }
        }

        // Check rewards for more specific descriptions
        var emote = quest.EmoteReward.ValueNullable;
        if (emote != null)
        {
            var emoteName = emote.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(emoteName))
                return (QuestCategory.Feature, $"Unlocks /{emoteName} emote");
        }

        var action = quest.ActionReward.ValueNullable;
        if (action != null)
        {
            var actionName = action.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(actionName))
                return (QuestCategory.Feature, $"Unlocks {actionName}");
        }

        if (quest.BeastTribe.RowId != 0)
        {
            var tribeName = quest.BeastTribe.ValueNullable?.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(tribeName))
                return (QuestCategory.Feature, $"Unlocks {tribeName} tribe");
        }

        return (QuestCategory.Feature, "");
    }

    public bool IsMsqQuest(uint rowId)
    {
        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (questSheet == null)
            return false;

        var quest = questSheet.GetRowOrDefault(rowId);
        return quest != null && quest.Value.EventIconType.RowId == 3;
    }

    public bool ArePrerequisitesMet(QuestData quest)
    {
        foreach (var prereqId in quest.PrerequisiteIds)
        {
            var (_, isCompleted, _) = GetPrerequisiteInfo(prereqId);
            if (!isCompleted)
                return false;
        }
        return true;
    }

    public int CompletedCount => BlueQuests.Count(q => q.IsCompleted);
    public int TotalCount => BlueQuests.Count;
    public float CompletionPercent => TotalCount > 0 ? (float)CompletedCount / TotalCount * 100f : 0f;
}
