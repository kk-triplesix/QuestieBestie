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
    public List<QuestData> SideQuests { get; } = [];

    private DateTime _lastRefresh = DateTime.MinValue;

    public QuestService()
    {
        LoadBlueQuests();
        LoadSideQuests();
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

            // Expansion
            var expansionId = quest.Expansion.RowId;
            var expansion = quest.Expansion.ValueNullable?.Name.ExtractText() ?? "";
            if (string.IsNullOrWhiteSpace(expansion))
                expansion = expansionId == 0 ? "A Realm Reborn" : $"Expansion {expansionId}";

            // Issuer coordinates for distance sorting
            var issuerLoc = quest.IssuerLocation.ValueNullable;
            var issuerX = issuerLoc?.X ?? 0f;
            var issuerZ = issuerLoc?.Z ?? 0f;
            var territoryId = issuerLoc?.Territory.RowId ?? 0u;

            // Determine category and unlock description
            var (category, unlocks) = DetermineQuestCategory(quest);

            // NPC name — IssuerStart is untyped RowRef, try to resolve as ENpcResident
            var npcName = "";
            try
            {
                var npcSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.ENpcResident>();
                var npcRow = npcSheet?.GetRowOrDefault(quest.IssuerStart.RowId);
                if (npcRow != null)
                    npcName = npcRow.Value.Singular.ExtractText();
            }
            catch { }

            // Rewards
            var rewardGil = quest.GilReward;
            uint rewardExp = 0;

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
                Expansion = expansion,
                ExpansionId = expansionId,
                IssuerX = issuerX,
                IssuerZ = issuerZ,
                TerritoryId = territoryId,
                Category = category,
                Unlocks = unlocks,
                NpcName = npcName,
                RewardGil = rewardGil,
                RewardExp = rewardExp,
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

        BuildQuestChains();
    }

    private void BuildQuestChains()
    {
        // Build chains by following PreviousQuest links between blue quests
        var visited = new HashSet<uint>();

        foreach (var quest in BlueQuests)
        {
            if (visited.Contains(quest.RowId) || !string.IsNullOrEmpty(quest.ChainName))
                continue;

            // Walk backwards to find the chain root
            var chain = new List<QuestData>();
            CollectChain(quest.RowId, chain, visited);

            if (chain.Count <= 1)
                continue;

            // Sort chain by level then RowId
            chain.Sort((a, b) => a.RequiredLevel != b.RequiredLevel
                ? a.RequiredLevel.CompareTo(b.RequiredLevel)
                : a.RowId.CompareTo(b.RowId));

            // Determine chain name from the first quest's unlock or the shared class
            var chainName = chain[0].Unlocks;
            if (string.IsNullOrEmpty(chainName))
            {
                var sharedClass = chain.Select(q => q.RequiredClassJob).Distinct().ToList();
                chainName = sharedClass.Count == 1 && sharedClass[0] != "All Classes"
                    ? $"{sharedClass[0]} Quests"
                    : $"{chain[0].Name} Chain";
            }

            for (var i = 0; i < chain.Count; i++)
            {
                chain[i].ChainName = chainName;
                chain[i].ChainIndex = i + 1;
            }
        }
    }

    private void CollectChain(uint rowId, List<QuestData> chain, HashSet<uint> visited)
    {
        if (!visited.Add(rowId) || !BlueQuestLookup.TryGetValue(rowId, out var quest))
            return;

        chain.Add(quest);

        // Follow quests that have this quest as a prerequisite
        foreach (var other in BlueQuests)
        {
            if (other.PrerequisiteIds.Contains(rowId))
                CollectChain(other.RowId, chain, visited);
        }

        // Follow this quest's prerequisites if they're blue quests
        foreach (var prereqId in quest.PrerequisiteIds)
        {
            if (BlueQuestLookup.ContainsKey(prereqId))
                CollectChain(prereqId, chain, visited);
        }
    }

    private void LoadSideQuests()
    {
        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (questSheet == null)
            return;

        // Collect all blue quest prerequisite IDs that are NOT blue quests themselves
        var bluePrereqIds = BlueQuests
            .SelectMany(q => q.PrerequisiteIds)
            .Where(id => !BlueQuestLookup.ContainsKey(id))
            .ToHashSet();

        // Build reverse lookup: yellow quest RowId -> which blue quest(s) it unlocks
        var unlocksBlueQuest = new Dictionary<uint, string>();
        foreach (var bq in BlueQuests)
        {
            foreach (var prereqId in bq.PrerequisiteIds)
            {
                if (!BlueQuestLookup.ContainsKey(prereqId))
                    unlocksBlueQuest.TryAdd(prereqId, bq.Name);
            }
        }

        foreach (var quest in questSheet)
        {
            // Only yellow/side quests (EventIconType 1)
            if (quest.EventIconType.RowId != 1)
                continue;

            var name = quest.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (quest.IssuerLocation.RowId == 0)
                continue;

            // Determine if this quest is special
            var isSpecial = false;
            var specialTags = new List<string>();

            // Is it a prerequisite for a blue quest?
            if (unlocksBlueQuest.TryGetValue(quest.RowId, out var blueQuestName))
            {
                isSpecial = true;
                specialTags.Add($"Required for: {blueQuestName}");
            }

            // Emote reward
            var emote = quest.EmoteReward.ValueNullable;
            if (emote != null)
            {
                var emoteName = emote.Value.Name.ExtractText();
                if (!string.IsNullOrWhiteSpace(emoteName))
                { isSpecial = true; specialTags.Add($"Emote: /{emoteName}"); }
            }

            // General action reward (mount, companion related)
            for (var i = 0; i < 2; i++)
            {
                var ga = quest.GeneralActionReward[i].ValueNullable;
                if (ga != null)
                {
                    var gaName = ga.Value.Name.ExtractText();
                    if (!string.IsNullOrWhiteSpace(gaName))
                    { isSpecial = true; specialTags.Add($"Unlocks: {gaName}"); }
                }
            }

            // Beast tribe
            if (quest.BeastTribe.RowId != 0)
            {
                var tribeName = quest.BeastTribe.ValueNullable?.Name.ExtractText();
                if (!string.IsNullOrWhiteSpace(tribeName))
                { isSpecial = true; specialTags.Add($"Tribe: {tribeName}"); }
            }

            var classJob = quest.ClassJobCategory0.ValueNullable?.Name.ExtractText() ?? "All Classes";
            var location = quest.PlaceName.ValueNullable?.Name.ExtractText() ?? "";
            if (string.IsNullOrWhiteSpace(location))
                location = quest.IssuerLocation.ValueNullable?.Territory.ValueNullable?.PlaceName.ValueNullable?.Name.ExtractText() ?? "";

            var expansionId = quest.Expansion.RowId;
            var expansion = quest.Expansion.ValueNullable?.Name.ExtractText() ?? "";
            if (string.IsNullOrWhiteSpace(expansion))
                expansion = expansionId == 0 ? "A Realm Reborn" : $"Expansion {expansionId}";

            var issuerLoc = quest.IssuerLocation.ValueNullable;

            SideQuests.Add(new QuestData
            {
                RowId = quest.RowId,
                QuestId = (ushort)(quest.RowId & 0xFFFF),
                Name = name,
                IconId = (ushort)quest.Icon,
                RequiredLevel = (byte)quest.ClassJobLevel[0],
                RequiredClassJob = classJob,
                PrerequisiteIds = [],
                Location = location,
                Expansion = expansion,
                ExpansionId = expansionId,
                IssuerX = issuerLoc?.X ?? 0f,
                IssuerZ = issuerLoc?.Z ?? 0f,
                TerritoryId = issuerLoc?.Territory.RowId ?? 0u,
                Category = QuestCategory.Other,
                IsSpecial = isSpecial,
                SpecialTag = string.Join(" | ", specialTags),
            });
        }

        // Remove duplicates
        var dupes = SideQuests.GroupBy(q => q.Name).Where(g => g.Count() > 1)
            .SelectMany(g => g.OrderByDescending(q => q.RowId).Skip(1)).Select(q => q.RowId).ToHashSet();
        if (dupes.Count > 0) SideQuests.RemoveAll(q => dupes.Contains(q.RowId));
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
            foreach (var quest in SideQuests)
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

        // General action rewards (glamour, dye, materia melding, desynthesis, treasure maps, etc.)
        for (var i = 0; i < 2; i++)
        {
            var ga = quest.GeneralActionReward[i].ValueNullable;
            if (ga != null)
            {
                var gaName = ga.Value.Name.ExtractText();
                if (!string.IsNullOrWhiteSpace(gaName))
                    return (QuestCategory.Feature, $"Unlocks {gaName}");
            }
        }

        // Other rewards (Aether Current, etc.)
        var otherReward = quest.OtherReward.ValueNullable;
        if (otherReward != null)
        {
            var rewardName = otherReward.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(rewardName) && rewardName != "???")
                return (QuestCategory.Feature, $"Rewards {rewardName}");
        }

        // Emote reward
        var emote = quest.EmoteReward.ValueNullable;
        if (emote != null)
        {
            var emoteName = emote.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(emoteName))
                return (QuestCategory.Feature, $"Unlocks /{emoteName} emote");
        }

        // Action reward
        var action = quest.ActionReward.ValueNullable;
        if (action != null)
        {
            var actionName = action.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(actionName))
                return (QuestCategory.Feature, $"Unlocks {actionName}");
        }

        // Beast tribe
        if (quest.BeastTribe.RowId != 0)
        {
            var tribeName = quest.BeastTribe.ValueNullable?.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(tribeName))
                return (QuestCategory.Feature, $"Unlocks {tribeName} tribe");
        }

        // Known umbrella quests — Wandering Minstrel / NPC unlock quests that have no instance data
        var umbrellaResult = CheckUmbrellaQuest(quest);
        if (umbrellaResult.HasValue)
            return umbrellaResult.Value;

        // ScriptInstruction fallback — catches dungeon/trial/raid unlocks not in InstanceContentUnlock
        var hasUnlockInstruction = false;
        uint scriptInstanceId = 0;
        for (var i = 0; i < 50; i++)
        {
            try
            {
                var instruction = quest.QuestParams[i].ScriptInstruction.ExtractText();
                if (string.IsNullOrEmpty(instruction))
                    continue;

                if (instruction.StartsWith("INSTANCEDUNGEON") && quest.QuestParams[i].ScriptArg != 0)
                    scriptInstanceId = quest.QuestParams[i].ScriptArg;

                if (instruction is "UNLOCK_ADD_NEW_CONTENT_TO_CF" or "UNLOCK_DUNGEON"
                    || instruction.StartsWith("UNLOCK_ADD_NEW_CONTENT") || instruction.StartsWith("UNLOCK_DUNGEON"))
                    hasUnlockInstruction = true;
            }
            catch { break; }
        }

        if (hasUnlockInstruction && scriptInstanceId != 0)
        {
            var cfcSheet = Svc.Data.GetExcelSheet<ContentFinderCondition>();
            if (cfcSheet != null)
            {
                foreach (var cfc in cfcSheet)
                {
                    if (cfc.Content.RowId == scriptInstanceId && cfc.ContentLinkType == 1)
                    {
                        var contentName = cfc.Name.ExtractText();
                        if (!string.IsNullOrWhiteSpace(contentName))
                        {
                            return cfc.ContentType.RowId switch
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
        }

        // Even without instance ID, if UNLOCK instructions exist, mark as content unlock
        if (hasUnlockInstruction)
            return (QuestCategory.Dungeon, "Unlocks content");

        return (QuestCategory.Feature, "");
    }

    private static (QuestCategory Category, string Unlocks)? CheckUmbrellaQuest(Quest quest)
    {
        // Known umbrella quest mappings — quests whose unlock data is NOT in the Quest sheet
        // These use NPC dialogue (Wandering Minstrel etc.) to unlock content
        var name = quest.Name.ExtractText();

        // Wandering Minstrel — Extreme Trials + Ultimates per expansion
        if (name.Contains("Songs in the Key of Kugane", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, "Unlocks SB Extremes + UCoB/UWU/TEA Ultimates");

        if (name.Contains("Minstrel from Another Mother", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, "Unlocks ShB Extreme Trials (Titania/Innocence/Hades/WoL)");

        if (name.Contains("Weapon of Choice", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, "Unlocks ShB Weapon Extremes (Ruby/Emerald/Diamond)");

        if (name.Contains("I Wandered Sharlayan as a Minstrel", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, "Unlocks EW Extremes + DSR/TOP Ultimates");

        if (name.Contains("How the West Was Sung", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, "Unlocks DT Extremes + FRU Ultimate");

        // Faux Hollows / Unreal
        if (name.Contains("Fantastic Mr. Faux", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, "Unlocks Faux Hollows / Unreal Trials");

        // Alliance Raids
        if (name.Contains("Legacy of Allag", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Raid, "Unlocks Labyrinth of the Ancients");

        // Generic Minstrel fallback
        if (name.Contains("Minstrel", StringComparison.OrdinalIgnoreCase) && !name.Contains("Ballad", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, "Unlocks Extreme Trials via Wandering Minstrel");

        return null;
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

    public float GetDistanceToPlayer(QuestData quest)
    {
        #pragma warning disable CS0618
        var player = Svc.ClientState.LocalPlayer;
        #pragma warning restore CS0618
        if (player == null)
            return float.MaxValue;

        var playerTerritory = Svc.ClientState.TerritoryType;
        if (quest.TerritoryId != playerTerritory)
            return float.MaxValue;

        var dx = quest.IssuerX - player.Position.X;
        var dz = quest.IssuerZ - player.Position.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public List<QuestData> PlanRoute(List<QuestData> quests)
    {
        if (quests.Count <= 1)
            return quests;

        // Group by territory, sort groups by closest territory first, then nearest-neighbor within each group
        #pragma warning disable CS0618
        var player = Svc.ClientState.LocalPlayer;
        #pragma warning restore CS0618
        var playerTerritory = Svc.ClientState.TerritoryType;
        var playerX = player?.Position.X ?? 0f;
        var playerZ = player?.Position.Z ?? 0f;

        var groups = quests.GroupBy(q => q.TerritoryId).ToList();

        // Sort groups: current territory first, then by smallest distance in group
        groups.Sort((a, b) =>
        {
            if (a.Key == playerTerritory && b.Key != playerTerritory) return -1;
            if (b.Key == playerTerritory && a.Key != playerTerritory) return 1;
            return 0;
        });

        var result = new List<QuestData>();
        foreach (var group in groups)
        {
            var remaining = group.ToList();
            var curX = group.Key == playerTerritory ? playerX : remaining[0].IssuerX;
            var curZ = group.Key == playerTerritory ? playerZ : remaining[0].IssuerZ;

            while (remaining.Count > 0)
            {
                var nearest = remaining.OrderBy(q =>
                {
                    var dx = q.IssuerX - curX;
                    var dz = q.IssuerZ - curZ;
                    return dx * dx + dz * dz;
                }).First();

                result.Add(nearest);
                curX = nearest.IssuerX;
                curZ = nearest.IssuerZ;
                remaining.Remove(nearest);
            }
        }

        return result;
    }

    public void SendQuestChatLink(uint rowId)
    {
        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (questSheet == null) return;
        var quest = questSheet.GetRowOrDefault(rowId);
        if (quest == null) return;

        var issuerLocation = quest.Value.IssuerLocation.ValueNullable;
        if (issuerLocation == null) return;

        var level = issuerLocation.Value;
        var map = level.Map.ValueNullable;
        var territory = level.Territory.ValueNullable;
        if (map == null || territory == null) return;

        var mapCoords = MapUtil.WorldToMap(new Vector2(level.X, level.Z), map.Value);
        var payload = new MapLinkPayload(territory.Value.RowId, map.Value.RowId, mapCoords.X, mapCoords.Y);

        var name = quest.Value.Name.ExtractText();
        var msg = new Dalamud.Game.Text.SeStringHandling.SeStringBuilder()
            .AddText($"{name}: ")
            .Add(payload)
            .AddText($"{payload.PlaceName} ({mapCoords.X:F1}, {mapCoords.Y:F1})")
            .Add(RawPayload.LinkTerminator)
            .Build();

        Svc.Chat.Print(new Dalamud.Game.Text.XivChatEntry { Message = msg });
    }

    public (float DirectionRad, float Distance, string QuestName)? GetNearestTrackedQuestDirection(List<uint> trackedIds)
    {
        #pragma warning disable CS0618
        var player = Svc.ClientState.LocalPlayer;
        #pragma warning restore CS0618
        if (player == null || trackedIds.Count == 0)
            return null;

        var territory = Svc.ClientState.TerritoryType;
        QuestData? nearest = null;
        var minDist = float.MaxValue;

        foreach (var id in trackedIds)
        {
            if (!BlueQuestLookup.TryGetValue(id, out var q) || q.IsCompleted || q.TerritoryId != territory)
                continue;

            var dx = q.IssuerX - player.Position.X;
            var dz = q.IssuerZ - player.Position.Z;
            var dist = MathF.Sqrt(dx * dx + dz * dz);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = q;
            }
        }

        if (nearest == null)
            return null;

        var dirX = nearest.IssuerX - player.Position.X;
        var dirZ = nearest.IssuerZ - player.Position.Z;
        var angle = MathF.Atan2(dirX, dirZ);
        return (angle, minDist, nearest.Name);
    }

    public int CompletedCount => BlueQuests.Count(q => q.IsCompleted);
    public int TotalCount => BlueQuests.Count;
    public float CompletionPercent => TotalCount > 0 ? (float)CompletedCount / TotalCount * 100f : 0f;

    public record ExpansionStats(string Name, int Total, int Completed)
    {
        public float Percent => Total > 0 ? (float)Completed / Total * 100f : 0f;
    }

    public record CategoryStats(string Name, int Total, int Completed)
    {
        public float Percent => Total > 0 ? (float)Completed / Total * 100f : 0f;
    }

    public List<ExpansionStats> GetExpansionStats()
    {
        return BlueQuests
            .GroupBy(q => q.Expansion)
            .OrderBy(g => g.First().ExpansionId)
            .Select(g => new ExpansionStats(g.Key, g.Count(), g.Count(q => q.IsCompleted)))
            .ToList();
    }

    public List<CategoryStats> GetCategoryStats()
    {
        return BlueQuests
            .GroupBy(q => q.Category)
            .OrderBy(g => g.Key)
            .Select(g => new CategoryStats(g.Key.ToString(), g.Count(), g.Count(q => q.IsCompleted)))
            .ToList();
    }

    private HashSet<uint> _previouslyAvailable = [];

    public List<QuestData> CheckNewlyAvailable()
    {
        var nowAvailable = BlueQuests
            .Where(q => !q.IsCompleted && ArePrerequisitesMet(q))
            .Select(q => q.RowId)
            .ToHashSet();

        var newQuests = new List<QuestData>();
        if (_previouslyAvailable.Count > 0)
        {
            foreach (var id in nowAvailable)
            {
                if (!_previouslyAvailable.Contains(id) && BlueQuestLookup.TryGetValue(id, out var q))
                    newQuests.Add(q);
            }
        }

        _previouslyAvailable = nowAvailable;
        return newQuests;
    }
}
