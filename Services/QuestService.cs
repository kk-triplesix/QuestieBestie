using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using QuestieBestie.Models;

namespace QuestieBestie.Services;

public sealed class QuestService
{
    private static readonly HashSet<uint> BlueQuestEventIconTypes = [8, 10];

    public List<QuestData> BlueQuests { get; } = [];
    public Dictionary<uint, QuestData> BlueQuestLookup { get; } = [];
    public List<QuestData> SideQuests { get; } = [];
    public Dictionary<string, List<QuestData>> ChainLookup { get; } = [];
    public int SpecialSideQuestCount { get; private set; }

    private readonly ExcelSheet<Quest>? _questSheet;
    private readonly ExcelSheet<Quest>? _enQuestSheet;
    private Dictionary<uint, ContentFinderCondition>? _cfcByContentId;

    private DateTime _lastRefresh = DateTime.MinValue;
    private HashSet<uint>? _manuallyCompleted;

    // Cached stats - updated in RefreshCompletionStatus()
    private int _cachedCompletedCount;
    private int _cachedTotalCount;
    private float _cachedCompletionPercent;
    private List<ExpansionStats>? _cachedExpansionStats;
    private List<CategoryStats>? _cachedCategoryStats;

    public QuestService()
    {
        _questSheet = QuestieBestiePlugin.Data.GetExcelSheet<Quest>();
        _enQuestSheet = QuestieBestiePlugin.Data.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.English);
        LoadBlueQuests();
        LoadSideQuests();
        _cachedTotalCount = BlueQuests.Count;
        QuestieBestiePlugin.Log.Information("QuestService: {Blue} blue quests, {Side} side quests, {Chains} chains loaded",
            BlueQuests.Count, SideQuests.Count, ChainLookup.Count);
    }

    public void SetManuallyCompleted(HashSet<uint> manuallyCompleted)
    {
        _manuallyCompleted = manuallyCompleted;
    }

    private void LoadBlueQuests()
    {
        // Use English sheet for categorization/dedup, client sheet for display names
        var enSheet = _enQuestSheet;
        var localSheet = _questSheet;
        if (enSheet == null || localSheet == null)
            return;

        var npcSheet = QuestieBestiePlugin.Data.GetExcelSheet<ENpcResident>();

        foreach (var quest in enSheet)
        {
            if (quest.RowId == 65536)
                continue;

            if (!BlueQuestEventIconTypes.Contains(quest.EventIconType.RowId))
                continue;

            var enName = quest.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(enName))
                continue;

            // Display name from client language
            var localQuest = localSheet.GetRowOrDefault(quest.RowId);
            var name = localQuest?.Name.ExtractText() ?? enName;
            if (string.IsNullOrWhiteSpace(name))
                name = enName;

            // Skip seasonal/event quests (Moonfire Faire, crossovers, etc.)
            if (quest.Festival.RowId != 0)
                continue;

            // Skip repeatable quests (daily beast tribe quests etc.)
            if (quest.RepeatIntervalType != 0)
                continue;

            // Skip removed/deprecated quests (no valid quest giver location)
            if (quest.IssuerLocation.RowId == 0)
                continue;

            var prereqs = new List<uint>();
            if (quest.PreviousQuest[0].RowId != 0) prereqs.Add(quest.PreviousQuest[0].RowId);
            if (quest.PreviousQuest[1].RowId != 0) prereqs.Add(quest.PreviousQuest[1].RowId);
            if (quest.PreviousQuest[2].RowId != 0) prereqs.Add(quest.PreviousQuest[2].RowId);

            var classJob = quest.ClassJobCategory0.ValueNullable?.Name.ExtractText() ?? Loc.Get("data.allClasses");
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

            // Determine category and unlock description (use local quest for display names)
            var localQuestForCategory = localSheet.GetRowOrDefault(quest.RowId);
            var (category, unlocks) = DetermineQuestCategory(quest, localQuestForCategory);

            // NPC name — IssuerStart is untyped RowRef, try to resolve as ENpcResident
            var npcName = "";
            try
            {
                var npcRow = npcSheet?.GetRowOrDefault(quest.IssuerStart.RowId);
                if (npcRow != null)
                    npcName = npcRow.Value.Singular.ExtractText();
            }
            catch (Exception ex) { QuestieBestiePlugin.Log.Verbose("NPC resolution failed for quest {Id}: {Error}", quest.RowId, ex.Message); }

            // Rewards
            var rewardGil = quest.GilReward;
            uint rewardExp = 0;

            var questData = new QuestData
            {
                RowId = quest.RowId,
                QuestId = (ushort)(quest.RowId & 0xFFFF),
                Name = name,
                EnglishName = enName,
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

        // Pre-check completion using both QuestManager and IUnlockState
        unsafe
        {
            var qm = QuestManager.Instance();
            if (qm != null)
                foreach (var q in BlueQuests)
                    q.IsCompleted = QuestManager.IsQuestComplete(q.QuestId);
        }

        // Cross-check: GC variants share EnglishName + RequiredLevel, mark all complete if any is
        unsafe
        {
            var qm = QuestManager.Instance();
            if (qm != null)
            {
                var nameGroups = BlueQuests
                    .GroupBy(q => (q.EnglishName, q.RequiredLevel))
                    .Where(g => g.Count() >= 2 && g.Count() <= 4 && g.Any(q => q.IsCompleted));
                foreach (var group in nameGroups)
                    foreach (var q in group.Where(q => !q.IsCompleted))
                        q.IsCompleted = true;
            }
        }

        // Remove duplicates: prefer completed quest, then highest RowId
        var toRemove = new HashSet<uint>();

        // 1. Same name + same level → GC variants, keep completed or newest
        foreach (var group in BlueQuests
            .GroupBy(q => (q.EnglishName, q.RequiredLevel))
            .Where(g => g.Count() >= 2 && g.Count() <= 4))
        {
            foreach (var old in group.OrderByDescending(q => q.IsCompleted).ThenByDescending(q => q.RowId).Skip(1))
                toRemove.Add(old.RowId);
        }

        // 2. Same unlock target → keep completed, or newest
        // Only for specific content unlocks (e.g. same dungeon name), not generic/chain descriptions
        foreach (var group in BlueQuests
            .Where(q => !string.IsNullOrEmpty(q.Unlocks) && q.Unlocks != Loc.Get("unlock.feature")
                && q.Unlocks != Loc.Get("unlock.content")
                && !q.Unlocks.Contains(Loc.Get("unlock.jobAbility"))
                && !q.Unlocks.Contains(Loc.Get("unlock.questChain"))
                && !q.Unlocks.Contains(Loc.Get("unlock.contentChain"))
                && q.Category is QuestCategory.JobUnlock or QuestCategory.Dungeon or QuestCategory.Trial or QuestCategory.Raid)
            .GroupBy(q => q.Unlocks)
            .Where(g => g.Count() >= 2 && g.Count() <= 4))
        {
            foreach (var old in group.OrderByDescending(q => q.IsCompleted).ThenByDescending(q => q.RowId).Skip(1))
                toRemove.Add(old.RowId);
        }

        // 3. GC variants: quests with same unlock + same level = GC variants (keep one)
        // Only applies to small groups (2-3) at same level to avoid false positives
        foreach (var group in BlueQuests
            .Where(q => !toRemove.Contains(q.RowId) && !string.IsNullOrEmpty(q.Unlocks)
                && q.Unlocks != Loc.Get("unlock.feature") && !q.Unlocks.Contains(Loc.Get("unlock.jobAbility"))
                && !q.Unlocks.Contains(Loc.Get("unlock.questChain")) && !q.Unlocks.StartsWith(Loc.Get("unlock.chain"))
                && !q.Unlocks.StartsWith(Loc.Get("unlock.leadsTo")))
            .GroupBy(q => (q.Unlocks, q.RequiredLevel))
            .Where(g => g.Count() >= 2 && g.Count() <= 4))
        {
            var anyCompleted = group.Any(q => q.IsCompleted);
            if (anyCompleted)
                foreach (var q in group)
                    q.IsCompleted = true;

            foreach (var old in group.OrderByDescending(q => q.IsCompleted).ThenByDescending(q => q.RowId).Skip(1))
                toRemove.Add(old.RowId);
        }

        // Transfer removed variants' QuestIds to the surviving quest
        if (toRemove.Count > 0)
        {
            // Group by EnglishName+Level to find survivors and removed variants
            foreach (var group in BlueQuests.GroupBy(q => (q.EnglishName, q.RequiredLevel)).Where(g => g.Any(q => toRemove.Contains(q.RowId))))
            {
                var survivor = group.FirstOrDefault(q => !toRemove.Contains(q.RowId));
                if (survivor == null) continue;
                var altIds = group.Where(q => toRemove.Contains(q.RowId)).Select(q => q.QuestId).ToList();
                if (altIds.Count > 0)
                    survivor.AlternateQuestIds = altIds.ToArray();
            }

            BlueQuests.RemoveAll(q => toRemove.Contains(q.RowId));
            foreach (var id in toRemove)
                BlueQuestLookup.Remove(id);
        }

        BuildQuestChains();
        PropagateChainUnlocks();
    }

    private void PropagateChainUnlocks()
    {
        // Group quests by chain name
        var chains = BlueQuests
            .Where(q => !string.IsNullOrEmpty(q.ChainName))
            .GroupBy(q => q.ChainName)
            .ToList();

        foreach (var chain in chains)
        {
            // Find the best unlock description in the chain (most specific, not generic)
            var bestUnlock = chain
                .Where(q => !string.IsNullOrEmpty(q.Unlocks)
                    && q.Unlocks != Loc.Get("unlock.feature")
                    && !q.Unlocks.EndsWith(Loc.Get("unlock.questChain"))
                    && !q.Unlocks.EndsWith(Loc.Get("unlock.jobAbility")))
                .Select(q => (q.Category, q.Unlocks))
                .FirstOrDefault();

            if (bestUnlock == default || string.IsNullOrEmpty(bestUnlock.Unlocks))
                continue;

            // Propagate to all quests in the chain that have a generic description
            foreach (var quest in chain)
            {
                if (quest.Unlocks == Loc.Get("unlock.feature") || quest.Unlocks == Loc.Get("unlock.contentChain")
                    || quest.Unlocks.EndsWith(Loc.Get("unlock.questChain")))
                {
                    quest.Unlocks = $"{Loc.Get("unlock.chain")} {bestUnlock.Unlocks}";
                    if (bestUnlock.Category is QuestCategory.Dungeon or QuestCategory.Trial or QuestCategory.Raid)
                        quest.Category = bestUnlock.Category;
                }
            }
        }

        // Also propagate for non-chain quests: if a blue quest's prerequisite chain leads to a dungeon unlock
        foreach (var quest in BlueQuests)
        {
            if (quest.Unlocks != Loc.Get("unlock.feature") || quest.PrerequisiteIds.Length == 0)
                continue;

            // Check if any quest that has THIS quest as a prerequisite has a specific unlock
            var dependent = BlueQuests.FirstOrDefault(q =>
                q.PrerequisiteIds.Contains(quest.RowId)
                && !string.IsNullOrEmpty(q.Unlocks)
                && q.Unlocks != Loc.Get("unlock.feature"));

            if (dependent != null)
            {
                quest.Unlocks = $"{Loc.Get("unlock.leadsTo")} {dependent.Unlocks}";
                if (dependent.Category is QuestCategory.Dungeon or QuestCategory.Trial or QuestCategory.Raid)
                    quest.Category = dependent.Category;
            }
        }
    }

    private void BuildQuestChains()
    {
        // Build reverse lookup: prerequisiteId -> quests that depend on it
        var dependents = new Dictionary<uint, List<QuestData>>();
        foreach (var q in BlueQuests)
            foreach (var pid in q.PrerequisiteIds)
                if (BlueQuestLookup.ContainsKey(pid))
                    (dependents.TryGetValue(pid, out var list) ? list : (dependents[pid] = [])).Add(q);

        // Build chains by following PreviousQuest links between blue quests
        var visited = new HashSet<uint>();

        foreach (var quest in BlueQuests)
        {
            if (visited.Contains(quest.RowId) || !string.IsNullOrEmpty(quest.ChainName))
                continue;

            // Walk backwards to find the chain root
            var chain = new List<QuestData>();
            CollectChain(quest.RowId, chain, visited, dependents);

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
                chainName = sharedClass.Count == 1 && sharedClass[0] != Loc.Get("data.allClasses")
                    ? $"{sharedClass[0]} {Loc.Get("chain.quests")}"
                    : $"{chain[0].Name} {Loc.Get("chain.chain")}";
            }

            for (var i = 0; i < chain.Count; i++)
            {
                chain[i].ChainName = chainName;
                chain[i].ChainIndex = i + 1;
            }
        }

        // Build ChainLookup for fast chain queries
        foreach (var quest in BlueQuests)
        {
            if (string.IsNullOrEmpty(quest.ChainName))
                continue;
            if (!ChainLookup.TryGetValue(quest.ChainName, out var list))
                ChainLookup[quest.ChainName] = list = [];
            list.Add(quest);
        }
    }

    private void CollectChain(uint rowId, List<QuestData> chain, HashSet<uint> visited, Dictionary<uint, List<QuestData>> dependents)
    {
        if (!visited.Add(rowId) || !BlueQuestLookup.TryGetValue(rowId, out var quest))
            return;

        chain.Add(quest);

        // Follow quests that have this quest as a prerequisite (O(1) lookup)
        if (dependents.TryGetValue(rowId, out var deps))
            foreach (var dep in deps)
                CollectChain(dep.RowId, chain, visited, dependents);

        // Follow this quest's prerequisites if they're blue quests
        foreach (var prereqId in quest.PrerequisiteIds)
        {
            if (BlueQuestLookup.ContainsKey(prereqId))
                CollectChain(prereqId, chain, visited, dependents);
        }
    }

    private void LoadSideQuests()
    {
        var questSheet = QuestieBestiePlugin.Data.GetExcelSheet<Quest>();
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
                specialTags.Add($"{Loc.Get("side.requiredFor")} {blueQuestName}");
            }

            // Emote reward
            var emote = quest.EmoteReward.ValueNullable;
            if (emote != null)
            {
                var emoteName = emote.Value.Name.ExtractText();
                if (!string.IsNullOrWhiteSpace(emoteName))
                { isSpecial = true; specialTags.Add($"{Loc.Get("side.emote")} /{emoteName}"); }
            }

            // General action reward (mount, companion related)
            for (var i = 0; i < 2; i++)
            {
                var ga = quest.GeneralActionReward[i].ValueNullable;
                if (ga != null)
                {
                    var gaName = ga.Value.Name.ExtractText();
                    if (!string.IsNullOrWhiteSpace(gaName))
                    { isSpecial = true; specialTags.Add($"{Loc.Get("side.unlocks")} {gaName}"); }
                }
            }

            // Beast tribe
            if (quest.BeastTribe.RowId != 0)
            {
                var tribeName = quest.BeastTribe.ValueNullable?.Name.ExtractText();
                if (!string.IsNullOrWhiteSpace(tribeName))
                { isSpecial = true; specialTags.Add($"{Loc.Get("side.tribe")} {tribeName}"); }
            }

            var classJob = quest.ClassJobCategory0.ValueNullable?.Name.ExtractText() ?? Loc.Get("data.allClasses");
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

        SpecialSideQuestCount = SideQuests.Count(q => q.IsSpecial);
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
            {
                quest.IsCompleted = QuestManager.IsQuestComplete(quest.QuestId);
                // GC variants: check alternate QuestIds (removed during dedup)
                if (!quest.IsCompleted && quest.AlternateQuestIds.Length > 0)
                {
                    foreach (var altId in quest.AlternateQuestIds)
                    {
                        if (QuestManager.IsQuestComplete(altId))
                        {
                            quest.IsCompleted = true;
                            break;
                        }
                    }
                }
                // Manual completion override
                if (!quest.IsCompleted && _manuallyCompleted != null && _manuallyCompleted.Contains(quest.RowId))
                    quest.IsCompleted = true;
            }

            foreach (var quest in SideQuests)
                quest.IsCompleted = QuestManager.IsQuestComplete(quest.QuestId);
        }

        // Update cached stats
        _cachedCompletedCount = BlueQuests.Count(q => q.IsCompleted);
        _cachedTotalCount = BlueQuests.Count;
        _cachedCompletionPercent = _cachedTotalCount > 0 ? (float)_cachedCompletedCount / _cachedTotalCount * 100f : 0f;
        _cachedExpansionStats = null;
        _cachedCategoryStats = null;
    }

    public (string Name, bool IsCompleted, bool IsBlueQuest) GetPrerequisiteInfo(uint rowId)
    {
        if (BlueQuestLookup.TryGetValue(rowId, out var blueQuest))
            return (blueQuest.Name, blueQuest.IsCompleted, true);

        if (_questSheet == null)
            return (Loc.Get("data.unknown"), false, false);

        var quest = _questSheet.GetRowOrDefault(rowId);
        if (quest == null)
            return (Loc.Get("data.unknown"), false, false);

        var name = quest.Value.Name.ExtractText();
        if (string.IsNullOrWhiteSpace(name))
            return (Loc.Get("data.unknown"), false, false);

        bool isCompleted;
        unsafe
        {
            var qm = QuestManager.Instance();
            isCompleted = qm != null && QuestManager.IsQuestComplete((ushort)(rowId & 0xFFFF));
        }
        if (!isCompleted && _manuallyCompleted != null && _manuallyCompleted.Contains(rowId))
            isCompleted = true;

        return (name, isCompleted, false);
    }

    public void OpenQuestOnMap(uint rowId)
    {
        if (_questSheet == null)
            return;

        var quest = _questSheet.GetRowOrDefault(rowId);
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
        QuestieBestiePlugin.GameGui.OpenMapWithMapLink(payload);
    }

    public List<PrerequisiteNode> GetPrerequisiteTree(uint rowId, int maxDepth = 10)
    {
        var visited = new HashSet<uint>();
        return BuildPrerequisiteTree(rowId, visited, maxDepth);
    }

    private List<PrerequisiteNode> BuildPrerequisiteTree(uint rowId, HashSet<uint> visited, int depth)
    {
        if (depth <= 0 || _questSheet == null)
            return [];

        var quest = _questSheet.GetRowOrDefault(rowId);
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
            if (name == Loc.Get("data.unknown"))
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

    private (QuestCategory Category, string Unlocks) DetermineQuestCategory(Quest quest, Quest? localQuest = null)
    {
        // Use local (client language) quest for display names, fall back to English
        var displayQuest = localQuest ?? quest;

        // Job unlock
        var jobUnlock = displayQuest.ClassJobUnlock.ValueNullable;
        if (jobUnlock != null)
        {
            var jobName = jobUnlock.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(jobName))
                return (QuestCategory.JobUnlock, $"{Loc.Get("unlock.unlocks")} {jobName}");
        }

        // Instance content unlock (dungeon/trial/raid)
        if (quest.InstanceContentUnlock.RowId != 0)
        {
            var result = LookupInstanceContent(quest.InstanceContentUnlock.RowId);
            if (result.HasValue)
                return result.Value;

            // Fallback: InstanceContentUnlock is set but no CFC match found
            return (QuestCategory.Dungeon, Loc.Get("unlock.content"));
        }

        // General action rewards (glamour, dye, materia melding, desynthesis, treasure maps, etc.)
        for (var i = 0; i < 2; i++)
        {
            var ga = displayQuest.GeneralActionReward[i].ValueNullable;
            if (ga != null)
            {
                var gaName = ga.Value.Name.ExtractText();
                if (!string.IsNullOrWhiteSpace(gaName))
                    return (QuestCategory.Feature, $"{Loc.Get("unlock.unlocks")} {gaName}");
            }
        }

        // Other rewards (Aether Current, etc.)
        var otherReward = displayQuest.OtherReward.ValueNullable;
        if (otherReward != null)
        {
            var rewardName = otherReward.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(rewardName) && rewardName != "???")
                return (QuestCategory.Feature, $"{Loc.Get("unlock.rewards")} {rewardName}");
        }

        // Emote reward
        var emote = displayQuest.EmoteReward.ValueNullable;
        if (emote != null)
        {
            var emoteName = emote.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(emoteName))
                return (QuestCategory.Feature, $"{Loc.Get("unlock.unlocks")} /{emoteName}");
        }

        // Action reward
        var action = displayQuest.ActionReward.ValueNullable;
        if (action != null)
        {
            var actionName = action.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(actionName))
                return (QuestCategory.Feature, $"{Loc.Get("unlock.unlocks")} {actionName}");
        }

        // Beast tribe
        if (quest.BeastTribe.RowId != 0)
        {
            var tribeName = displayQuest.BeastTribe.ValueNullable?.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(tribeName))
                return (QuestCategory.Feature, $"{Loc.Get("unlock.unlocks")} {tribeName}");
        }

        // Known umbrella quests (quest is already English)
        var umbrellaResult = CheckUmbrellaQuest(quest);
        if (umbrellaResult.HasValue)
            return umbrellaResult.Value;

        // ScriptInstruction fallback — catches dungeon/trial/raid unlocks not in InstanceContentUnlock
        var hasUnlockInstruction = false;
        uint scriptInstanceId = 0;
        var paramCount = quest.QuestParams.Count;
        for (var i = 0; i < paramCount; i++)
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

        if (hasUnlockInstruction && scriptInstanceId != 0)
        {
            var scriptResult = LookupInstanceContent(scriptInstanceId);
            if (scriptResult.HasValue)
                return scriptResult.Value;
        }

        // Even without instance ID, if UNLOCK instructions exist, mark as content unlock
        if (hasUnlockInstruction)
            return (QuestCategory.Dungeon, Loc.Get("unlock.content"));

        // Manual lookup (quest is already English)
        var manual = QuestUnlockData.Lookup(quest.Name.ExtractText());
        if (manual.HasValue)
            return manual.Value;

        // Smart fallback — use context to generate a meaningful description
        return DetermineFromContext(quest, displayQuest);
    }

    private static (QuestCategory Category, string Unlocks) DetermineFromContext(Quest quest, Quest displayQuest)
    {
        // Job quest chain (has class requirement but didn't unlock a new job)
        if (quest.ClassJobRequired.RowId != 0)
        {
            var jobName = displayQuest.ClassJobRequired.ValueNullable?.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(jobName))
                return (QuestCategory.JobUnlock, $"{jobName} ({Loc.Get("unlock.jobAbility")})");
        }

        // Role quest chains
        var questName = quest.Name.ExtractText();
        if (questName.Contains("Role Quest", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Feature, Loc.Get("unlock.roleQuest"));

        // Tribal/Beast tribe related (follow-up quests in tribal chains)
        if (quest.BeastTribe.RowId != 0)
        {
            var tribeName = displayQuest.BeastTribe.ValueNullable?.Name.ExtractText();
            return (QuestCategory.Feature, !string.IsNullOrWhiteSpace(tribeName) ? $"{tribeName} ({Loc.Get("unlock.tribalQuest")})" : Loc.Get("unlock.tribalQuest"));
        }

        // Check if quest has instance content prerequisites — likely part of a content chain
        for (var i = 0; i < 3; i++)
        {
            if (quest.InstanceContent[i].RowId != 0)
                return (QuestCategory.Feature, Loc.Get("unlock.contentChain"));
        }

        // Check JournalGenre for categorization hints (match against English, display local)
        var genre = quest.JournalGenre.ValueNullable;
        if (genre != null)
        {
            var genreName = genre.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(genreName))
            {
                if (genreName.Contains("Chronicles", StringComparison.OrdinalIgnoreCase))
                    return (QuestCategory.Feature, Loc.Get("unlock.chronicles"));
                if (genreName.Contains("Crystalline", StringComparison.OrdinalIgnoreCase) ||
                    genreName.Contains("Studium", StringComparison.OrdinalIgnoreCase) ||
                    genreName.Contains("Wachumeqimeqi", StringComparison.OrdinalIgnoreCase))
                    return (QuestCategory.Feature, Loc.Get("unlock.crafterDelivery"));
                if (genreName.Contains("Hildibrand", StringComparison.OrdinalIgnoreCase))
                    return (QuestCategory.Feature, Loc.Get("unlock.hildibrand"));
                if (genreName.Contains("Relic", StringComparison.OrdinalIgnoreCase) ||
                    genreName.Contains("Zodiac", StringComparison.OrdinalIgnoreCase) ||
                    genreName.Contains("Anima", StringComparison.OrdinalIgnoreCase) ||
                    genreName.Contains("Resistance", StringComparison.OrdinalIgnoreCase) ||
                    genreName.Contains("Manderville", StringComparison.OrdinalIgnoreCase))
                    return (QuestCategory.Feature, Loc.Get("unlock.relic"));
                var localGenreName = displayQuest.JournalGenre.ValueNullable?.Name.ExtractText();
                return (QuestCategory.Feature, $"{localGenreName ?? genreName}");
            }
        }

        return (QuestCategory.Feature, Loc.Get("unlock.feature"));
    }

    private (QuestCategory Category, string Unlocks)? LookupInstanceContent(uint instanceContentId)
    {
        // Lazy-init CFC index on first use
        if (_cfcByContentId == null)
        {
            var cfcSheet = QuestieBestiePlugin.Data.GetExcelSheet<ContentFinderCondition>();
            if (cfcSheet == null)
                return null;

            _cfcByContentId = [];
            foreach (var cfc in cfcSheet)
            {
                if (cfc.Content.RowId != 0 && !string.IsNullOrWhiteSpace(cfc.Name.ExtractText()))
                    _cfcByContentId.TryAdd(cfc.Content.RowId, cfc);
            }
        }

        if (!_cfcByContentId.TryGetValue(instanceContentId, out var match))
            return null;

        var contentName = match.Name.ExtractText();
        var contentTypeId = match.ContentType.RowId;
        return contentTypeId switch
        {
            2 => (QuestCategory.Dungeon, $"{Loc.Get("unlock.unlocks")} {contentName}"),
            4 => (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} {contentName}"),
            5 or 28 or 37 => (QuestCategory.Raid, $"{Loc.Get("unlock.unlocks")} {contentName}"),
            _ => (QuestCategory.Other, $"{Loc.Get("unlock.unlocks")} {contentName}"),
        };
    }

    private static (QuestCategory Category, string Unlocks)? CheckUmbrellaQuest(Quest quest)
        => CheckUmbrellaQuestByName(quest.Name.ExtractText());

    private static (QuestCategory Category, string Unlocks)? CheckUmbrellaQuestByName(string name)
    {

        // Wandering Minstrel — Extreme Trials + Ultimates per expansion
        if (name.Contains("Songs in the Key of Kugane", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} SB Extremes + UCoB/UWU/TEA Ultimates");

        if (name.Contains("Minstrel from Another Mother", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} ShB Extreme Trials (Titania/Innocence/Hades/WoL)");

        if (name.Contains("Weapon of Choice", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} ShB Weapon Extremes (Ruby/Emerald/Diamond)");

        if (name.Contains("I Wandered Sharlayan as a Minstrel", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} EW Extremes + DSR/TOP Ultimates");

        if (name.Contains("How the West Was Sung", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} DT Extremes + FRU Ultimate");

        // Faux Hollows / Unreal
        if (name.Contains("Fantastic Mr. Faux", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} Faux Hollows / Unreal Trials");

        // Alliance Raids
        if (name.Contains("Legacy of Allag", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Raid, $"{Loc.Get("unlock.unlocks")} Labyrinth of the Ancients");

        // Generic Minstrel fallback
        if (name.Contains("Minstrel", StringComparison.OrdinalIgnoreCase) && !name.Contains("Ballad", StringComparison.OrdinalIgnoreCase))
            return (QuestCategory.Trial, $"{Loc.Get("unlock.unlocks")} Extreme Trials via Wandering Minstrel");

        return null;
    }

    public bool IsMsqQuest(uint rowId)
    {
        if (_questSheet == null)
            return false;

        var quest = _questSheet.GetRowOrDefault(rowId);
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

    private static unsafe Vector3? GetPlayerPosition()
    {
        var player = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        return player == null ? null : player->Position;
    }

    public float GetDistanceToPlayer(QuestData quest)
    {
        var pos = GetPlayerPosition();
        if (pos == null)
            return float.MaxValue;

        var playerTerritory = QuestieBestiePlugin.ClientState.TerritoryType;
        if (quest.TerritoryId != playerTerritory)
            return float.MaxValue;

        var dx = quest.IssuerX - pos.Value.X;
        var dz = quest.IssuerZ - pos.Value.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public List<QuestData> PlanRoute(List<QuestData> quests)
    {
        if (quests.Count <= 1)
            return quests;

        // Group by territory, sort groups by closest territory first, then nearest-neighbor within each group
        var pos = GetPlayerPosition();
        var playerTerritory = QuestieBestiePlugin.ClientState.TerritoryType;
        var playerX = pos?.X ?? 0f;
        var playerZ = pos?.Z ?? 0f;

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
        if (!BlueQuestLookup.TryGetValue(rowId, out var questData))
            return;

        if (_questSheet == null) return;
        var quest = _questSheet.GetRowOrDefault(rowId);
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
        var unlockInfo = !string.IsNullOrEmpty(questData.Unlocks) ? $" - {questData.Unlocks}" : "";

        // Show locally with clickable map link
        var msg = new Dalamud.Game.Text.SeStringHandling.SeStringBuilder()
            .AddUiForeground("[QuestieBestie] ", 35)
            .AddUiForeground(name, 34)
            .AddText($" (Lv.{questData.RequiredLevel}){unlockInfo} ")
            .Add(payload)
            .AddText($"{payload.PlaceName} ({mapCoords.X:F1}, {mapCoords.Y:F1})")
            .Build();
        QuestieBestiePlugin.Chat.Print(new Dalamud.Game.Text.XivChatEntry { Message = msg });

        // Copy quest info to clipboard so user can paste in chat for others
        ImGui.SetClipboardText($"{name} (Lv.{questData.RequiredLevel}){unlockInfo} - {payload.PlaceName} ({mapCoords.X:F1}, {mapCoords.Y:F1})");

        // Confirmation
        QuestieBestiePlugin.Chat.Print(new Dalamud.Game.Text.XivChatEntry
        {
            Message = new Dalamud.Game.Text.SeStringHandling.SeStringBuilder()
                .AddUiForeground("[QuestieBestie] ", 35)
                .AddText(Loc.Get("chat.copied"))
                .Build(),
        });
    }

    public (float DirectionRad, float Distance, string QuestName)? GetNearestTrackedQuestDirection(List<uint> trackedIds)
    {
        var pos = GetPlayerPosition();
        if (pos == null || trackedIds.Count == 0)
            return null;

        var territory = QuestieBestiePlugin.ClientState.TerritoryType;
        QuestData? nearest = null;
        var minDist = float.MaxValue;

        foreach (var id in trackedIds)
        {
            if (!BlueQuestLookup.TryGetValue(id, out var q) || q.IsCompleted || q.TerritoryId != territory)
                continue;

            var dx = q.IssuerX - pos.Value.X;
            var dz = q.IssuerZ - pos.Value.Z;
            var dist = MathF.Sqrt(dx * dx + dz * dz);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = q;
            }
        }

        if (nearest == null)
            return null;

        var dirX = nearest.IssuerX - pos.Value.X;
        var dirZ = nearest.IssuerZ - pos.Value.Z;
        var angle = MathF.Atan2(dirX, dirZ);
        return (angle, minDist, nearest.Name);
    }

    public int CompletedCount => _cachedCompletedCount;
    public int TotalCount => _cachedTotalCount;
    public float CompletionPercent => _cachedCompletionPercent;

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
        return _cachedExpansionStats ??= BlueQuests
            .GroupBy(q => q.Expansion)
            .OrderBy(g => g.First().ExpansionId)
            .Select(g => new ExpansionStats(g.Key, g.Count(), g.Count(q => q.IsCompleted)))
            .ToList();
    }

    public List<CategoryStats> GetCategoryStats()
    {
        return _cachedCategoryStats ??= BlueQuests
            .GroupBy(q => q.Category)
            .OrderBy(g => g.Key)
            .Select(g => new CategoryStats(g.Key.ToString(), g.Count(), g.Count(q => q.IsCompleted)))
            .ToList();
    }
}
