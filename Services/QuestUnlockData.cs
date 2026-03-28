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

        // ── Crafting/Gathering Classes (EN + DE) ────────────────────
        ("Way of the Carpenter", QuestCategory.JobUnlock, "Unlocks Carpenter"),
        ("Way of the Blacksmith", QuestCategory.JobUnlock, "Unlocks Blacksmith"),
        ("Way of the Armorer", QuestCategory.JobUnlock, "Unlocks Armorer"),
        ("Way of the Goldsmith", QuestCategory.JobUnlock, "Unlocks Goldsmith"),
        ("Way of the Leatherworker", QuestCategory.JobUnlock, "Unlocks Leatherworker"),
        ("Way of the Weaver", QuestCategory.JobUnlock, "Unlocks Weaver"),
        ("Way of the Alchemist", QuestCategory.JobUnlock, "Unlocks Alchemist"),
        ("Way of the Culinarian", QuestCategory.JobUnlock, "Unlocks Culinarian"),
        ("Way of the Miner", QuestCategory.JobUnlock, "Unlocks Miner"),
        ("Way of the Botanist", QuestCategory.JobUnlock, "Unlocks Botanist"),
        ("Way of the Fisher", QuestCategory.JobUnlock, "Unlocks Fisher"),
        ("Gilde der Zimmerer", QuestCategory.JobUnlock, "Unlocks Carpenter"),
        ("Gilde der Grobschmiede", QuestCategory.JobUnlock, "Unlocks Blacksmith"),
        ("Gilde der Plattner", QuestCategory.JobUnlock, "Unlocks Armorer"),
        ("Gilde der Goldschmiede", QuestCategory.JobUnlock, "Unlocks Goldsmith"),
        ("Gilde der Gerber", QuestCategory.JobUnlock, "Unlocks Leatherworker"),
        ("Gilde der Weber", QuestCategory.JobUnlock, "Unlocks Weaver"),
        ("Gilde der Alchemisten", QuestCategory.JobUnlock, "Unlocks Alchemist"),
        ("Gilde der Gourmets", QuestCategory.JobUnlock, "Unlocks Culinarian"),
        ("Gilde der Minenarbeiter", QuestCategory.JobUnlock, "Unlocks Miner"),
        ("Gilde der G\u00e4rtner", QuestCategory.JobUnlock, "Unlocks Botanist"),
        ("Gilde der Fischer", QuestCategory.JobUnlock, "Unlocks Fisher"),
        ("Gilde der Maschinisten", QuestCategory.JobUnlock, "Unlocks Machinist"),

        // ── Levequests per Expansion ────────────────────────────────
        ("Leves of", QuestCategory.Feature, "Unlocks Levequests"),
        ("Ishgardian Leves", QuestCategory.Feature, "Unlocks HW Levequests"),
        ("Leves of Kugane", QuestCategory.Feature, "Unlocks SB Levequests"),
        ("Leves of the Crystarium", QuestCategory.Feature, "Unlocks ShB Levequests"),
        ("Leves of Tuliyollal", QuestCategory.Feature, "Unlocks DT Levequests"),

        // ── HW Job Unlocks (DE) ─────────────────────────────────────
        ("d\u00fcsteres Schauspiel", QuestCategory.JobUnlock, "Unlocks Dark Knight"),
        ("Wiege der Astrologie", QuestCategory.JobUnlock, "Unlocks Astrologian"),
        ("Savior of Skysteel", QuestCategory.JobUnlock, "Unlocks Machinist"),
        ("Out of the Shadows", QuestCategory.JobUnlock, "Unlocks Dark Knight"),
        ("Stairway to the Heavens", QuestCategory.JobUnlock, "Unlocks Astrologian"),

        // ── German specific unlocks ─────────────────────────────────
        ("Wiedererweckte Erinnerungen", QuestCategory.Feature, "Unlocks New Game+"),
        ("Willkommen zu den Triple Triad-Turnieren", QuestCategory.Feature, "Unlocks Triple Triad Tournaments"),
        ("Trautes Heim, Inselgl\u00fcck", QuestCategory.Feature, "Unlocks Island Sanctuary"),
        ("Abstieg in die Katakomben", QuestCategory.Dungeon, "Unlocks Palace of the Dead"),
        ("Rowenas Wacht", QuestCategory.Feature, "Unlocks Collectables (SB)"),
        ("Frontbericht", QuestCategory.Feature, "Unlocks Bozja Field Notes"),
        ("verzweifelte Schreiber", QuestCategory.Feature, "Unlocks Resistance Weapons (ShB)"),
        ("Lebenszeichen", QuestCategory.Feature, "Unlocks Dwarf Beast Tribe"),
        ("Helfende H\u00e4nde", QuestCategory.Feature, "Unlocks Dwarf Beast Tribe quest chain"),
        ("Papas Paradoxie", QuestCategory.Feature, "Unlocks Dwarf Beast Tribe quest chain"),
        ("Ausgefuchste Assistenz", QuestCategory.Feature, "Unlocks Dwarf Beast Tribe quest chain"),
        ("Papas persistente Paradoxie", QuestCategory.Feature, "Unlocks Dwarf Beast Tribe quest chain"),
        ("Gl\u00fcck auf f\u00fcr Komra", QuestCategory.Raid, "Unlocks Copied Factory (YoRHa)"),
        ("Orden f\u00fcr deine Brust", QuestCategory.Feature, "Unlocks Zadnor Field Notes"),
        ("Traumwetter", QuestCategory.Feature, "Unlocks Shared FATE (ShB)"),

        // ── German: Gold Saucer ─────────────────────────────────────
        ("Kartenfieber", QuestCategory.Feature, "Unlocks Mini Cactpot"),
        ("kleines St\u00fcck vom Gl\u00fcck", QuestCategory.Feature, "Unlocks Mini Cactpot (weekly)"),
        ("gro\u00dfes St\u00fcck vom Gl\u00fcck", QuestCategory.Feature, "Unlocks Jumbo Cactpot"),

        // ── German: Performance ─────────────────────────────────────
        ("Wenn Herzen musizieren", QuestCategory.Feature, "Unlocks Performance Actions"),

        // ── German: Squadrons ───────────────────────────────────────
        ("neues Kommando", QuestCategory.Feature, "Unlocks Adventurer Squadrons"),

        // ── German: Orchestrion ─────────────────────────────────────
        ("Lieder sind Erinnerungen", QuestCategory.Feature, "Unlocks Duty Recorder"),

        // ── German: Levequests ──────────────────────────────────────
        ("Ishgarder Freibriefe", QuestCategory.Feature, "Unlocks HW Levequests"),
        ("Freibriefe von Kugane", QuestCategory.Feature, "Unlocks SB Levequests"),
        ("Freibriefe von Crystarium", QuestCategory.Feature, "Unlocks ShB Levequests"),
        ("Freibriefe von Tuliyollal", QuestCategory.Feature, "Unlocks DT Levequests"),

        // ── German: Sightseeing Logs ────────────────────────────────
        ("Unbekannter Osten", QuestCategory.Feature, "Unlocks Sightseeing Log (SB)"),
        ("Unbekanntes Norvrandt", QuestCategory.Feature, "Unlocks Sightseeing Log (ShB)"),
        ("Unbekannte Welten", QuestCategory.Feature, "Unlocks Sightseeing Log (EW)"),
        ("Unbekanntes Tural", QuestCategory.Feature, "Unlocks Sightseeing Log (DT)"),

        // ── German: Facet/Deliveries ────────────────────────────────
        ("Handwerker ist K\u00f6nig", QuestCategory.Feature, "Unlocks Facet Deliveries (ShB)"),
        ("Klauber von Alt-Sharlayan", QuestCategory.Feature, "Unlocks Studium Deliveries (EW)"),
        ("Expansion des Hauses der Wunder", QuestCategory.Feature, "Unlocks Radz-at-Han Deliveries"),
        ("Handel mit Rhodina", QuestCategory.Feature, "Unlocks DT Custom Deliveries"),

        // ── German: Misc ────────────────────────────────────────────
        ("Milliths Cousin", QuestCategory.Feature, "Unlocks Clan Hunt Board (HW)"),
        ("Klein, aber oho", QuestCategory.Feature, "Unlocks Pixie Beast Tribe"),
        ("Musikalische Inspiration", QuestCategory.Feature, "Unlocks Performance Actions (EW)"),
        ("Gelegenheitsauftr\u00e4ge", QuestCategory.Feature, "Unlocks Variant Dungeons"),
        ("Poesie im fernen Westen", QuestCategory.Trial, "Unlocks DT Extremes + FRU Ultimate"),
        ("Balladenk\u00e4nge in Kugane", QuestCategory.Trial, "Unlocks SB Extremes + UCoB/UWU/TEA Ultimates"),
        ("Fahrende S\u00e4nger des ersten Splitters", QuestCategory.Trial, "Unlocks ShB Extreme Trials"),
        ("Verschollene Zwillingsschwester", QuestCategory.Feature, "Unlocks DT side content"),

        // ── Triple Triad Tournaments ────────────────────────────────
        ("Triple Triad Tournament", QuestCategory.Feature, "Unlocks Triple Triad Tournaments"),

        // ── Orchestrion / Duty Recorder ─────────────────────────────
        ("Completion", QuestCategory.Feature, "Unlocks Duty Completion mode"),

        // ── Sightseeing per Expansion (alternate names) ─────────────
        ("Sightsee", QuestCategory.Feature, "Unlocks Sightseeing Log"),

        // ── New Game+ ───────────────────────────────────────────────
        ("Reviving the Past", QuestCategory.Feature, "Unlocks New Game+"),

        // ── Collectables (per expansion) ────────────────────────────
        ("Rowena's House of Splendors", QuestCategory.Feature, "Unlocks Collectables (SB)"),
        ("The Boutique Always Wins", QuestCategory.Feature, "Unlocks Collectables (ShB)"),
        ("Collecting Collegium", QuestCategory.Feature, "Unlocks Collectables (EW)"),
        ("Expanding House of Splendors", QuestCategory.Feature, "Unlocks Collectables (DT)"),

        // ── Dwarf/Pixie/Qitari Beast Tribes (ShB) ──────────────────
        ("A Sulky Sylph", QuestCategory.Feature, "Unlocks Sylph Beast Tribe"),
        ("You Have Selected Regicide", QuestCategory.Feature, "Unlocks Amalj'aa Beast Tribe"),
        ("A Bad Bladder", QuestCategory.Feature, "Unlocks Kobold Beast Tribe"),
        ("They Came from the Deep", QuestCategory.Feature, "Unlocks Sahagin Beast Tribe"),
        ("Ixali Imbroglio", QuestCategory.Feature, "Unlocks Ixal Beast Tribe"),
        ("The Bittersweet", QuestCategory.Feature, "Unlocks Pixie Beast Tribe"),
        ("A Pact with the Pack", QuestCategory.Feature, "Unlocks Vath Beast Tribe"),
        ("Moogles Moved In", QuestCategory.Feature, "Unlocks Moogle Beast Tribe"),
        ("Peace for Thanalan", QuestCategory.Feature, "Unlocks Amalj'aa crafting"),
        ("A Sundrop Dance", QuestCategory.Feature, "Unlocks Qitari Beast Tribe"),
        ("It's Dwarfin' Time", QuestCategory.Feature, "Unlocks Dwarf Beast Tribe"),
        ("Allies Beyond the Rift", QuestCategory.Feature, "Unlocks Arkasodara Beast Tribe"),
        ("The Gift of Worship", QuestCategory.Feature, "Unlocks Omicron Beast Tribe"),
        ("Loporrit Love", QuestCategory.Feature, "Unlocks Loporrit Beast Tribe"),
        ("The Pelupelu Way", QuestCategory.Feature, "Unlocks Pelupelu Beast Tribe"),

        // ── YoRHa/Nier Raids ────────────────────────────────────────
        ("Word about Komra", QuestCategory.Raid, "Unlocks Copied Factory (YoRHa)"),
        ("A Beacon for the Black", QuestCategory.Raid, "Unlocks Puppets' Bunker (YoRHa)"),
        ("Konogg the Road Less Traveled", QuestCategory.Raid, "Unlocks Tower at Paradigm's Breach (YoRHa)"),

        // ── Ivalice Raids ───────────────────────────────────────────
        ("Dramatis Personae", QuestCategory.Raid, "Unlocks Royal City of Rabanastre"),
        ("A City Fallen", QuestCategory.Raid, "Unlocks Ridorana Lighthouse"),
        ("The City of Lost Angels", QuestCategory.Raid, "Unlocks Orbonne Monastery"),

        // ── Myths of the Realm ──────────────────────────────────────
        ("A Mission in Mor Dhona", QuestCategory.Raid, "Unlocks Aglaia"),
        ("An Alliance of Equals", QuestCategory.Raid, "Unlocks Euphrosyne"),
        ("A Pact Most Dire", QuestCategory.Raid, "Unlocks Thaleia"),

        // ── Echoes of Vana'diel ─────────────────────────────────────
        ("Vana'diel", QuestCategory.Raid, "Unlocks Jeuno: The First Walk"),

        // ── Void Ark / Shadow of Mhach ──────────────────────────────
        ("Sky Pirates", QuestCategory.Raid, "Unlocks Void Ark"),

        // ── NieR: Automata Robots (DT) ──────────────────────────────
        ("Arcadion", QuestCategory.Raid, "Unlocks Arcadion raid"),

        // ── Optional Dungeons (exact EN quest names) ────────────────
        // HW
        ("Completion Complex", QuestCategory.Dungeon, "Unlocks The Twinning"),
        ("Brave New Companions", QuestCategory.Dungeon, "Unlocks Neverreap"),
        ("The Fractal Continuum", QuestCategory.Dungeon, "Unlocks The Fractal Continuum"),
        ("For Keep's Sake", QuestCategory.Dungeon, "Unlocks Saint Mocianne's Arboretum"),
        // SB
        ("An Auspicious Encounter", QuestCategory.Dungeon, "Unlocks Kugane Castle"),
        ("Temple of the Fist", QuestCategory.Dungeon, "Unlocks Temple of the Fist"),
        ("Not without Incident", QuestCategory.Dungeon, "Unlocks Hells' Lid"),
        ("The Swallow's Compass", QuestCategory.Dungeon, "Unlocks The Swallow's Compass"),
        // ShB
        ("By the Time You Hear This", QuestCategory.Dungeon, "Unlocks The Twinning"),
        ("Where All Roads Lead", QuestCategory.Dungeon, "Unlocks Akadaemia Anyder"),
        // EW
        ("Cutting the Cheese", QuestCategory.Dungeon, "Unlocks Smileton"),
        ("Where No Loporrit Has Gone Before", QuestCategory.Dungeon, "Unlocks Stigma Dreamscape"),
        ("Alzadaal's Legacy", QuestCategory.Dungeon, "Unlocks Alzadaal's Legacy"),
        ("In the Shadow of the Warring Triad", QuestCategory.Dungeon, "Unlocks The Fell Court of Troia"),
        // DT
        ("Something Stray in the Neighborhood", QuestCategory.Dungeon, "Unlocks The Strayborough Deadwalk"),
        ("It Belongs in a Museum", QuestCategory.Dungeon, "Unlocks Tender Valley"),

        // ── DT Content ──────────────────────────────────────────────
        ("Lost Twin", QuestCategory.Feature, "Unlocks DT content"),
        ("Rhodina", QuestCategory.Feature, "Unlocks DT Custom Deliveries"),

        // ── Occult Crescent ─────────────────────────────────────────
        ("Occult", QuestCategory.Feature, "Unlocks Occult Crescent"),

        // ── Bozja ───────────────────────────────────────────────────
        ("The Bozja Incident", QuestCategory.Feature, "Unlocks Save the Queen storyline"),
        ("In the Field", QuestCategory.Feature, "Unlocks Field Notes"),

        // ── Duty Roulettes ──────────────────────────────────────────
        ("Duty Roulette", QuestCategory.Feature, "Unlocks Duty Roulette"),
        ("A Trial for What It's Worth", QuestCategory.Feature, "Unlocks Trial Roulette"),

        // ── Ventures & Retainers ────────────────────────────────────
        ("The Scions of the Seventh Dawn", QuestCategory.Feature, "Unlocks Retainers"),

        // ── Grand Company ───────────────────────────────────────────
        ("The Company You Keep", QuestCategory.Feature, "Unlocks Grand Company"),

        // ── Airship Travel ──────────────────────────────────────────
        ("The Ul'dahn Envoy", QuestCategory.Feature, "Unlocks Airship Travel"),
        ("The Gridanian Envoy", QuestCategory.Feature, "Unlocks Airship Travel"),
        ("The Lominsan Envoy", QuestCategory.Feature, "Unlocks Airship Travel"),

        // ── Explorer Mode ───────────────────────────────────────────
        ("Duty Explorer", QuestCategory.Feature, "Unlocks Explorer Mode"),

        // ── Trust System ────────────────────────────────────────────
        ("Shadowbringers", QuestCategory.Feature, "Unlocks Trust System"),

        // ── Mentor System ───────────────────────────────────────────
        ("Mentor", QuestCategory.Feature, "Unlocks Mentor Roulette"),

        // ── Shared FATE ─────────────────────────────────────────────
        ("A Shared FATE", QuestCategory.Feature, "Unlocks Shared FATE Rewards"),

        // ── Unrestricted Party / Min IL ─────────────────────────────
        ("Completion", QuestCategory.Feature, "Unlocks Duty Completion mode"),
    ];
}
