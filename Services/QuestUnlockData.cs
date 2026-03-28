using QuestieBestie.Models;

namespace QuestieBestie.Services;

public static class QuestUnlockData
{
    public static (QuestCategory Category, string Unlocks)? Lookup(string questName)
    {
        foreach (var (pattern, category, unlocks) in ManualEntries)
        {
            if (questName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return (category, unlocks);
        }
        return null;
    }

    private static readonly (string Pattern, QuestCategory Category, string Unlocks)[] ManualEntries =
    [
        // ── Gold Saucer ─────────────────────────────────────────────
        ("It Could Happen to You", QuestCategory.Feature, "Unlocks Gold Saucer"),
        ("Triple Triad Trial", QuestCategory.Feature, "Unlocks Triple Triad"),
        ("So You Want to Be a Jockey", QuestCategory.Feature, "Unlocks Chocobo Racing"),
        ("Passion for Fashion", QuestCategory.Feature, "Unlocks Fashion Report"),
        ("Every Little Thing She Does Is Mahjong", QuestCategory.Feature, "Unlocks Doman Mahjong"),
        ("Scratch It Rich", QuestCategory.Feature, "Unlocks Mini Cactpot"),
        ("Hitting the Cactpot", QuestCategory.Feature, "Unlocks Jumbo Cactpot"),

        // ── Housing ─────────────────────────────────────────────────
        ("Where the Heart Is", QuestCategory.Feature, "Unlocks Housing District"),
        ("I Dream of Shirogane", QuestCategory.Feature, "Unlocks Shirogane Housing"),
        ("Ascending to Empyreum", QuestCategory.Feature, "Unlocks Empyreum Housing"),

        // ── System Features ─────────────────────────────────────────
        ("Rising to the Challenge", QuestCategory.Feature, "Unlocks Challenge Log"),
        ("An Ill-conceived Venture", QuestCategory.Feature, "Unlocks Retainer Ventures"),
        ("Beauty Is Only Scalp Deep", QuestCategory.Feature, "Unlocks Aesthetician"),
        ("Treasures and Tribulations", QuestCategory.Feature, "Unlocks Treasure Hunt"),
        ("Keeping Up with the Aliapohs", QuestCategory.Feature, "Unlocks Wondrous Tails"),
        ("Precious Reclamation", QuestCategory.Feature, "Unlocks Doman Enclave"),
        ("Seeking Sanctuary", QuestCategory.Feature, "Unlocks Island Sanctuary"),
        ("A Cosmic Homecoming", QuestCategory.Feature, "Unlocks Cosmic Exploration"),
        ("An Odd Job", QuestCategory.Feature, "Unlocks Variant Dungeons"),
        ("Recall of Duty", QuestCategory.Feature, "Unlocks Duty Recorder"),
        ("The Ties That Bind", QuestCategory.Feature, "Unlocks Eternal Bonding"),
        ("Plucking the Heartstrings", QuestCategory.Feature, "Unlocks Performance Actions"),
        ("Of Errant Epistles", QuestCategory.Feature, "Unlocks Delivery Moogle Quests"),
        ("The Real Folk Blues", QuestCategory.Feature, "Unlocks Masked Carnivale"),

        // ── Sightseeing Logs ────────────────────────────────────────
        ("A Sight to Behold", QuestCategory.Feature, "Unlocks Sightseeing Log (ARR)"),
        ("Sights of the North", QuestCategory.Feature, "Unlocks Sightseeing Log (HW)"),
        ("Sights of Crimson and Dawn", QuestCategory.Feature, "Unlocks Sightseeing Log (SB)"),
        ("Sights of the First", QuestCategory.Feature, "Unlocks Sightseeing Log (ShB)"),
        ("Sights of the End", QuestCategory.Feature, "Unlocks Sightseeing Log (EW)"),
        ("Sights of the West", QuestCategory.Feature, "Unlocks Sightseeing Log (DT)"),

        // ── Hunts ───────────────────────────────────────────────────
        ("Let the Hunt Begin", QuestCategory.Feature, "Unlocks The Hunt (ARR)"),
        ("Let the Clan Hunt Begin", QuestCategory.Feature, "Unlocks Clan Hunts (HW)"),
        ("Better Bill Hunting", QuestCategory.Feature, "Unlocks Clan Hunt Rank 2 (HW)"),
        ("Top Marks", QuestCategory.Feature, "Unlocks Clan Hunt Rank 3 (HW)"),
        ("Elite and Dangerous", QuestCategory.Feature, "Unlocks Elite Marks (HW)"),
        ("One-star Veteran Clan Hunt", QuestCategory.Feature, "Unlocks Veteran Clan Hunt (SB)"),
        ("Two-star Veteran Clan Hunt", QuestCategory.Feature, "Unlocks Veteran Hunt Rank 2 (SB)"),
        ("Three-star Veteran Clan Hunt", QuestCategory.Feature, "Unlocks Veteran Hunt Rank 3 (SB)"),
        ("Elite Veteran Clan Hunt", QuestCategory.Feature, "Unlocks Elite Marks (SB)"),
        ("Nuts to You", QuestCategory.Feature, "Unlocks Nutsy Clan Hunt (ShB)"),
        ("Two Nuts Too Nutty", QuestCategory.Feature, "Unlocks Nutsy Hunt Rank 2 (ShB)"),
        ("How Do You Like Three Nuts", QuestCategory.Feature, "Unlocks Nutsy Hunt Rank 3 (ShB)"),
        ("Too Many Nutters", QuestCategory.Feature, "Unlocks Elite Marks (ShB)"),
        ("The Hunt for Specimens", QuestCategory.Feature, "Unlocks Guildship Hunt (EW)"),
        ("That Specimen Came from the Moon", QuestCategory.Feature, "Unlocks Associate Marks (EW)"),
        ("A Hunt for the Ages", QuestCategory.Feature, "Unlocks Senior Marks (EW)"),
        ("Perfect Specimens", QuestCategory.Feature, "Unlocks Elite Marks (EW)"),
        ("A New Dawn, a New Hunt", QuestCategory.Feature, "Unlocks Dawn Marks (DT)"),
        ("Why We Hunt", QuestCategory.Feature, "Unlocks Intermediate Marks (DT)"),
        ("Hunting the Hunter", QuestCategory.Feature, "Unlocks Advanced Marks (DT)"),
        ("The Hunt Goes On", QuestCategory.Feature, "Unlocks Elite Marks (DT)"),

        // ── PvP ─────────────────────────────────────────────────────
        ("A Pup No Longer", QuestCategory.Feature, "Unlocks PvP / Wolves' Den"),
        ("The Crystal", QuestCategory.Feature, "Unlocks Crystalline Conflict"),
        ("Like Civilized Men and Women", QuestCategory.Feature, "Unlocks Frontline"),
        ("Earning Your Wings", QuestCategory.Feature, "Unlocks Rival Wings"),

        // ── Chocobo ─────────────────────────────────────────────────
        ("My Little Chocobo", QuestCategory.Feature, "Unlocks Chocobo Mount"),
        ("My Feisty Little Chocobo", QuestCategory.Feature, "Unlocks Chocobo Companion"),
        ("Bird in Hand", QuestCategory.Feature, "Unlocks Chocobo Raising"),

        // ── Deep Dungeons ───────────────────────────────────────────
        ("The House That Death Built", QuestCategory.Dungeon, "Unlocks Palace of the Dead"),
        ("Knocking on Heaven's Door", QuestCategory.Dungeon, "Unlocks Heaven-on-High"),
        ("Delve into Myth", QuestCategory.Dungeon, "Unlocks Eureka Orthos"),
        ("Pilgrimage of Light", QuestCategory.Dungeon, "Unlocks Pilgrim's Traverse"),

        // ── Custom Deliveries ───────────────────────────────────────
        ("Arms Wide Open", QuestCategory.Feature, "Unlocks Custom Deliveries (Zhloe)"),
        ("None Forgotten, None Forsaken", QuestCategory.Feature, "Unlocks Custom Deliveries (M'naago)"),
        ("The Seaweed Is Always Greener", QuestCategory.Feature, "Unlocks Custom Deliveries (Kurenai)"),
        ("Between a Rock and the Hard Place", QuestCategory.Feature, "Unlocks Custom Deliveries (Adkiragh)"),
        ("Oh, Beehive Yourself", QuestCategory.Feature, "Unlocks Custom Deliveries (Kai-Shirr)"),
        ("O Crafter, My Crafter", QuestCategory.Feature, "Unlocks Custom Deliveries (Ehll Tou)"),
        ("You Can Count on It", QuestCategory.Feature, "Unlocks Custom Deliveries (Charlemend)"),
        ("Of Mothers and Merchants", QuestCategory.Feature, "Unlocks Custom Deliveries (Ameliance)"),
        ("That's So Anden", QuestCategory.Feature, "Unlocks Custom Deliveries (Anden)"),
        ("A Request of One's Own", QuestCategory.Feature, "Unlocks Custom Deliveries (Margrat)"),
        ("Laying New Tracks", QuestCategory.Feature, "Unlocks Custom Deliveries (Nitowikwe)"),

        // ── Deliveries (Crystalline Mean / Studium / Wachumeqimeqi) ─
        ("The Crystalline Mean", QuestCategory.Feature, "Unlocks Crystarium Deliveries"),
        ("The Faculty", QuestCategory.Feature, "Unlocks Studium Deliveries"),
        ("Wrought in Wachumeqimeqi", QuestCategory.Feature, "Unlocks Wachumeqimeqi Deliveries"),

        // ── Field Operations ────────────────────────────────────────
        ("And We Shall Call It Eureka", QuestCategory.Feature, "Unlocks Eureka Anemos"),
        ("And We Shall Call It Pagos", QuestCategory.Feature, "Unlocks Eureka Pagos"),
        ("And We Shall Call It Pyros", QuestCategory.Feature, "Unlocks Eureka Pyros"),
        ("And We Shall Call It Hydatos", QuestCategory.Feature, "Unlocks Eureka Hydatos"),
        ("Where Eagles Nest", QuestCategory.Feature, "Unlocks Bozjan Southern Front"),
        ("Fit for a Queen", QuestCategory.Raid, "Unlocks Delubrum Reginae"),
        ("A New Playing Field", QuestCategory.Feature, "Unlocks Zadnor"),
        ("One Last Hurrah", QuestCategory.Feature, "Unlocks Occult Crescent"),

        // ── Ishgard Restoration ─────────────────────────────────────
        ("Towards the Firmament", QuestCategory.Feature, "Unlocks Ishgardian Restoration"),
        ("Mislaid Plans", QuestCategory.Feature, "Unlocks Skysteel Tools"),

        // ── Relic Weapons ───────────────────────────────────────────
        ("The Weaponsmith of Legend", QuestCategory.Feature, "Unlocks Zodiac Weapons (ARR)"),
        ("An Unexpected Proposal", QuestCategory.Feature, "Unlocks Anima Weapons (HW)"),
        ("Hail to the Queen", QuestCategory.Feature, "Unlocks Resistance Weapons (ShB)"),
        ("Make It a Manderville", QuestCategory.Feature, "Unlocks Manderville Weapons (EW)"),
        ("Arcane Artistry", QuestCategory.Feature, "Unlocks Phantom Weapons (DT)"),
        ("An Original Improvement", QuestCategory.Feature, "Unlocks Splendorous Tools (EW)"),

        // ── Crafting/Gathering ──────────────────────────────────────
        ("Inscrutable Tastes", QuestCategory.Feature, "Unlocks Collectables"),
        ("A New Fishing Ex-spear-ience", QuestCategory.Feature, "Unlocks Spearfishing"),
        ("All the Fish in the Sea", QuestCategory.Feature, "Unlocks Ocean Fishing"),

        // ── Guildhests & Squadrons ──────────────────────────────────
        ("Simply the Hest", QuestCategory.Feature, "Unlocks Guildhests"),
        ("Squadron and Commander", QuestCategory.Feature, "Unlocks Adventurer Squadrons"),

        // ── Hildibrand ──────────────────────────────────────────────
        ("The Rise and Fall of Gentlemen", QuestCategory.Feature, "Unlocks Hildibrand Adventures"),
    ];
}
