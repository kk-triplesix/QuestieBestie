namespace QuestieBestie.Services;

public static class Loc
{
    private static string _lang = "en";

    public static void Init()
    {
        var clientLang = QuestieBestiePlugin.ClientState.ClientLanguage.ToString().ToLowerInvariant();
        _lang = clientLang switch
        {
            "german" => "de",
            "french" => "fr",
            "japanese" => "ja",
            _ => "en",
        };
    }

    public static string Get(string key) => Strings.TryGetValue(key, out var dict)
        ? dict.GetValueOrDefault(_lang, dict.GetValueOrDefault("en", key))
        : key;

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        // ── Tabs ────────────────────────────────────────────────────────
        ["tab.quests"] = new() { ["en"] = "Quests", ["de"] = "Quests", ["fr"] = "Qu\u00eates", ["ja"] = "\u30af\u30a8\u30b9\u30c8" },
        ["tab.aether"] = new() { ["en"] = "Aether Currents", ["de"] = "\u00c4therstr\u00f6me", ["fr"] = "Courants \u00e9th\u00e9r\u00e9s", ["ja"] = "\u98a8\u8108" },
        ["tab.duties"] = new() { ["en"] = "Duty Unlocks", ["de"] = "Inhalte", ["fr"] = "Missions", ["ja"] = "\u30b3\u30f3\u30c6\u30f3\u30c4" },
        ["tab.side"] = new() { ["en"] = "Side Quests", ["de"] = "Nebenquests", ["fr"] = "Qu\u00eates annexes", ["ja"] = "\u30b5\u30d6\u30af\u30a8\u30b9\u30c8" },
        ["tab.stats"] = new() { ["en"] = "Statistics", ["de"] = "Statistiken", ["fr"] = "Statistiques", ["ja"] = "\u7d71\u8a08" },

        // ── Header ──────────────────────────────────────────────────────
        ["header.subtitle"] = new() { ["en"] = "Blue Quest Tracker", ["de"] = "Blauer Quest Tracker", ["fr"] = "Suivi de qu\u00eates bleues", ["ja"] = "\u9752\u30af\u30a8\u30b9\u30c8\u30c8\u30e9\u30c3\u30ab\u30fc" },
        ["header.overlay"] = new() { ["en"] = "Overlay", ["de"] = "Overlay", ["fr"] = "Overlay", ["ja"] = "\u30aa\u30fc\u30d0\u30fc\u30ec\u30a4" },
        ["header.hideOverlay"] = new() { ["en"] = "Hide Overlay", ["de"] = "Overlay aus", ["fr"] = "Masquer", ["ja"] = "\u975e\u8868\u793a" },
        ["header.widget"] = new() { ["en"] = "Widget", ["de"] = "Widget", ["fr"] = "Widget", ["ja"] = "\u30a6\u30a3\u30b8\u30a7\u30c3\u30c8" },
        ["header.hideWidget"] = new() { ["en"] = "Hide Widget", ["de"] = "Widget aus", ["fr"] = "Masquer", ["ja"] = "\u975e\u8868\u793a" },
        ["header.settings"] = new() { ["en"] = "Settings", ["de"] = "Einstellungen", ["fr"] = "Param\u00e8tres", ["ja"] = "\u8a2d\u5b9a" },
        ["header.list"] = new() { ["en"] = "List:", ["de"] = "Liste:", ["fr"] = "Liste:", ["ja"] = "\u30ea\u30b9\u30c8:" },
        ["header.export"] = new() { ["en"] = "Export", ["de"] = "Exportieren", ["fr"] = "Exporter", ["ja"] = "\u30a8\u30af\u30b9\u30dd\u30fc\u30c8" },
        ["header.import"] = new() { ["en"] = "Import", ["de"] = "Importieren", ["fr"] = "Importer", ["ja"] = "\u30a4\u30f3\u30dd\u30fc\u30c8" },

        // ── Filters ─────────────────────────────────────────────────────
        ["filter.search"] = new() { ["en"] = "Search quests...", ["de"] = "Quests suchen...", ["fr"] = "Chercher...", ["ja"] = "\u691c\u7d22..." },
        ["filter.all"] = new() { ["en"] = "All", ["de"] = "Alle", ["fr"] = "Tous", ["ja"] = "\u3059\u3079\u3066" },
        ["filter.available"] = new() { ["en"] = "Available", ["de"] = "Verf\u00fcgbar", ["fr"] = "Disponible", ["ja"] = "\u53d7\u6ce8\u53ef" },
        ["filter.incomplete"] = new() { ["en"] = "Incomplete", ["de"] = "Offen", ["fr"] = "Incomplet", ["ja"] = "\u672a\u5b8c\u4e86" },
        ["filter.complete"] = new() { ["en"] = "Complete", ["de"] = "Erledigt", ["fr"] = "Termin\u00e9", ["ja"] = "\u5b8c\u4e86" },
        ["filter.nearest"] = new() { ["en"] = "Nearest", ["de"] = "N\u00e4chste", ["fr"] = "Proche", ["ja"] = "\u6700\u5bc4\u308a" },
        ["filter.specialOnly"] = new() { ["en"] = "Special Only", ["de"] = "Nur Besondere", ["fr"] = "Sp\u00e9cial", ["ja"] = "\u7279\u5225\u306e\u307f" },

        // ── Table Headers ───────────────────────────────────────────────
        ["col.quest"] = new() { ["en"] = "Quest", ["de"] = "Quest", ["fr"] = "Qu\u00eate", ["ja"] = "\u30af\u30a8\u30b9\u30c8" },
        ["col.level"] = new() { ["en"] = "Lv.", ["de"] = "Lv.", ["fr"] = "Nv.", ["ja"] = "Lv." },
        ["col.location"] = new() { ["en"] = "Location", ["de"] = "Ort", ["fr"] = "Lieu", ["ja"] = "\u5834\u6240" },
        ["col.classjob"] = new() { ["en"] = "Class/Job", ["de"] = "Klasse/Job", ["fr"] = "Classe", ["ja"] = "\u30af\u30e9\u30b9" },
        ["col.unlocks"] = new() { ["en"] = "Unlocks", ["de"] = "Schaltet frei", ["fr"] = "D\u00e9bloque", ["ja"] = "\u89e3\u653e" },
        ["col.special"] = new() { ["en"] = "Special", ["de"] = "Besonders", ["fr"] = "Sp\u00e9cial", ["ja"] = "\u7279\u5225" },
        ["col.npc"] = new() { ["en"] = "NPC", ["de"] = "NPC", ["fr"] = "PNJ", ["ja"] = "NPC" },

        // ── Context Menu ────────────────────────────────────────────────
        ["ctx.favorite"] = new() { ["en"] = "Add Favorite", ["de"] = "Favorit", ["fr"] = "Favori", ["ja"] = "\u304a\u6c17\u306b\u5165\u308a" },
        ["ctx.unfavorite"] = new() { ["en"] = "Remove Favorite", ["de"] = "Favorit entfernen", ["fr"] = "Retirer", ["ja"] = "\u89e3\u9664" },
        ["ctx.map"] = new() { ["en"] = "Show on Map", ["de"] = "Auf Karte zeigen", ["fr"] = "Voir sur la carte", ["ja"] = "\u30de\u30c3\u30d7\u306b\u8868\u793a" },
        ["ctx.chat"] = new() { ["en"] = "Send to Chat", ["de"] = "Im Chat teilen", ["fr"] = "Envoyer au chat", ["ja"] = "\u30c1\u30e3\u30c3\u30c8\u306b\u9001\u4fe1" },
        ["ctx.addTo"] = new() { ["en"] = "Add to", ["de"] = "Hinzuf\u00fcgen zu", ["fr"] = "Ajouter \u00e0", ["ja"] = "\u8ffd\u52a0" },
        ["ctx.newList"] = new() { ["en"] = "New list name...", ["de"] = "Neuer Listenname...", ["fr"] = "Nom de liste...", ["ja"] = "\u30ea\u30b9\u30c8\u540d..." },
        ["ctx.create"] = new() { ["en"] = "Create", ["de"] = "Erstellen", ["fr"] = "Cr\u00e9er", ["ja"] = "\u4f5c\u6210" },
        ["ctx.rename"] = new() { ["en"] = "Rename", ["de"] = "Umbenennen", ["fr"] = "Renommer", ["ja"] = "\u540d\u524d\u5909\u66f4" },
        ["ctx.delete"] = new() { ["en"] = "Delete List", ["de"] = "Liste l\u00f6schen", ["fr"] = "Supprimer", ["ja"] = "\u524a\u9664" },
        ["ctx.undo"] = new() { ["en"] = "Undo Remove", ["de"] = "R\u00fcckg\u00e4ngig", ["fr"] = "Annuler", ["ja"] = "\u5143\u306b\u623b\u3059" },
        ["ctx.remove"] = new() { ["en"] = "Remove from list", ["de"] = "Aus Liste entfernen", ["fr"] = "Retirer de la liste", ["ja"] = "\u30ea\u30b9\u30c8\u304b\u3089\u524a\u9664" },

        // ── Detail Window ───────────────────────────────────────────────
        ["detail.expansion"] = new() { ["en"] = "Expansion", ["de"] = "Erweiterung", ["fr"] = "Extension", ["ja"] = "\u62e1\u5f35" },
        ["detail.level"] = new() { ["en"] = "Level", ["de"] = "Level", ["fr"] = "Niveau", ["ja"] = "\u30ec\u30d9\u30eb" },
        ["detail.location"] = new() { ["en"] = "Location", ["de"] = "Ort", ["fr"] = "Lieu", ["ja"] = "\u5834\u6240" },
        ["detail.classjob"] = new() { ["en"] = "Class/Job", ["de"] = "Klasse/Job", ["fr"] = "Classe", ["ja"] = "\u30af\u30e9\u30b9" },
        ["detail.type"] = new() { ["en"] = "Type", ["de"] = "Typ", ["fr"] = "Type", ["ja"] = "\u30bf\u30a4\u30d7" },
        ["detail.unlocks"] = new() { ["en"] = "Unlocks", ["de"] = "Schaltet frei", ["fr"] = "D\u00e9bloque", ["ja"] = "\u89e3\u653e" },
        ["detail.chain"] = new() { ["en"] = "Chain", ["de"] = "Kette", ["fr"] = "Cha\u00eene", ["ja"] = "\u30c1\u30a7\u30fc\u30f3" },
        ["detail.npc"] = new() { ["en"] = "Quest Giver", ["de"] = "Questgeber", ["fr"] = "Donneur", ["ja"] = "\u4f9d\u983c\u4eba" },
        ["detail.rewards"] = new() { ["en"] = "Rewards", ["de"] = "Belohnungen", ["fr"] = "R\u00e9compenses", ["ja"] = "\u5831\u916c" },
        ["detail.notes"] = new() { ["en"] = "Notes", ["de"] = "Notizen", ["fr"] = "Notes", ["ja"] = "\u30e1\u30e2" },
        ["detail.prereqs"] = new() { ["en"] = "Prerequisites", ["de"] = "Voraussetzungen", ["fr"] = "Pr\u00e9requis", ["ja"] = "\u524d\u63d0\u6761\u4ef6" },
        ["detail.noPrereqs"] = new() { ["en"] = "No prerequisites.", ["de"] = "Keine Voraussetzungen.", ["fr"] = "Aucun pr\u00e9requis.", ["ja"] = "\u524d\u63d0\u6761\u4ef6\u306a\u3057" },
        ["detail.complete"] = new() { ["en"] = "Complete", ["de"] = "Erledigt", ["fr"] = "Termin\u00e9", ["ja"] = "\u5b8c\u4e86" },
        ["detail.incomplete"] = new() { ["en"] = "Incomplete", ["de"] = "Offen", ["fr"] = "Incomplet", ["ja"] = "\u672a\u5b8c\u4e86" },

        // ── Statistics ──────────────────────────────────────────────────
        ["stats.overall"] = new() { ["en"] = "Overall Progress", ["de"] = "Gesamtfortschritt", ["fr"] = "Progr\u00e8s global", ["ja"] = "\u5168\u4f53\u306e\u9032\u6357" },
        ["stats.byExpansion"] = new() { ["en"] = "By Expansion", ["de"] = "Nach Erweiterung", ["fr"] = "Par extension", ["ja"] = "\u62e1\u5f35\u5225" },
        ["stats.byType"] = new() { ["en"] = "By Type", ["de"] = "Nach Typ", ["fr"] = "Par type", ["ja"] = "\u30bf\u30a4\u30d7\u5225" },
        ["stats.chains"] = new() { ["en"] = "Quest Chains", ["de"] = "Questketten", ["fr"] = "Cha\u00eenes de qu\u00eates", ["ja"] = "\u30af\u30a8\u30b9\u30c8\u30c1\u30a7\u30fc\u30f3" },

        // ── Overlay ─────────────────────────────────────────────────────
        ["overlay.noQuests"] = new() { ["en"] = "No tracked quests.", ["de"] = "Keine getrackten Quests.", ["fr"] = "Aucune qu\u00eate suivie.", ["ja"] = "\u8ffd\u8de1\u30af\u30a8\u30b9\u30c8\u306a\u3057" },
        ["overlay.addHint"] = new() { ["en"] = "Right-click quests in main window to add.", ["de"] = "Rechtsklick im Hauptfenster zum Hinzuf\u00fcgen.", ["fr"] = "Clic droit dans la fen\u00eatre principale.", ["ja"] = "\u30e1\u30a4\u30f3\u30a6\u30a3\u30f3\u30c9\u30a6\u3067\u53f3\u30af\u30ea\u30c3\u30af" },

        // ── Settings ────────────────────────────────────────────────────
        ["settings.title"] = new() { ["en"] = "Overlay Settings", ["de"] = "Overlay Einstellungen", ["fr"] = "Param\u00e8tres Overlay", ["ja"] = "\u30aa\u30fc\u30d0\u30fc\u30ec\u30a4\u8a2d\u5b9a" },
        ["settings.general"] = new() { ["en"] = "General", ["de"] = "Allgemein", ["fr"] = "G\u00e9n\u00e9ral", ["ja"] = "\u4e00\u822c" },
        ["settings.colors"] = new() { ["en"] = "Colors", ["de"] = "Farben", ["fr"] = "Couleurs", ["ja"] = "\u30ab\u30e9\u30fc" },
        ["settings.behavior"] = new() { ["en"] = "Behavior", ["de"] = "Verhalten", ["fr"] = "Comportement", ["ja"] = "\u52d5\u4f5c" },
        ["settings.autoRemove"] = new() { ["en"] = "Auto-remove completed quests from lists", ["de"] = "Erledigte Quests automatisch entfernen", ["fr"] = "Retirer automatiquement les qu\u00eates termin\u00e9es", ["ja"] = "\u5b8c\u4e86\u3057\u305f\u30af\u30a8\u30b9\u30c8\u3092\u81ea\u52d5\u524a\u9664" },
        ["settings.chatNotify"] = new() { ["en"] = "Chat notifications for newly available quests", ["de"] = "Chat-Benachrichtigung bei neuen Quests", ["fr"] = "Notifications chat pour nouvelles qu\u00eates", ["ja"] = "\u65b0\u898f\u30af\u30a8\u30b9\u30c8\u306e\u30c1\u30e3\u30c3\u30c8\u901a\u77e5" },
        ["settings.soundNotify"] = new() { ["en"] = "Sound notification for newly available quests", ["de"] = "Sound-Benachrichtigung bei neuen Quests", ["fr"] = "Notification sonore", ["ja"] = "\u30b5\u30a6\u30f3\u30c9\u901a\u77e5" },
        ["settings.reset"] = new() { ["en"] = "Reset to Defaults", ["de"] = "Zur\u00fccksetzen", ["fr"] = "R\u00e9initialiser", ["ja"] = "\u30c7\u30d5\u30a9\u30eb\u30c8\u306b\u623b\u3059" },
        ["settings.syncWarning"] = new() { ["en"] = "WARNING", ["de"] = "WARNUNG", ["fr"] = "ATTENTION", ["ja"] = "\u8b66\u544a" },
        ["settings.syncDesc"] = new() { ["en"] = "All {0} manual completion changes will be permanently removed and the quest status will be synchronized with the game state.", ["de"] = "Alle {0} manuellen Completion-\u00c4nderungen werden unwiderruflich entfernt und der Quest-Status wird mit dem Spielstand synchronisiert.", ["fr"] = "Les {0} modifications manuelles seront d\u00e9finitivement supprim\u00e9es et le statut des qu\u00eates sera synchronis\u00e9 avec l'\u00e9tat du jeu.", ["ja"] = "\u624b\u52d5\u3067\u5909\u66f4\u3057\u305f{0}\u4ef6\u306e\u5b8c\u4e86\u30b9\u30c6\u30fc\u30bf\u30b9\u304c\u5b8c\u5168\u306b\u524a\u9664\u3055\u308c\u3001\u30b2\u30fc\u30e0\u306e\u72b6\u614b\u3068\u540c\u671f\u3055\u308c\u307e\u3059\u3002" },
        ["settings.syncIrreversible"] = new() { ["en"] = "This action cannot be undone!", ["de"] = "Dieser Vorgang kann nicht r\u00fcckg\u00e4ngig gemacht werden!", ["fr"] = "Cette action est irr\u00e9versible !", ["ja"] = "\u3053\u306e\u64cd\u4f5c\u306f\u5143\u306b\u623b\u305b\u307e\u305b\u3093\uff01" },
        ["settings.syncConfirm"] = new() { ["en"] = "Yes, remove all local changes", ["de"] = "Ja, alle lokalen \u00c4nderungen entfernen", ["fr"] = "Oui, supprimer toutes les modifications", ["ja"] = "\u306f\u3044\u3001\u3059\u3079\u3066\u306e\u30ed\u30fc\u30ab\u30eb\u5909\u66f4\u3092\u524a\u9664" },
        ["settings.syncCancel"] = new() { ["en"] = "Cancel", ["de"] = "Abbrechen", ["fr"] = "Annuler", ["ja"] = "\u30ad\u30e3\u30f3\u30bb\u30eb" },

        // ── Misc ────────────────────────────────────────────────────────
        ["misc.shown"] = new() { ["en"] = "shown", ["de"] = "angezeigt", ["fr"] = "affich\u00e9s", ["ja"] = "\u8868\u793a" },
        ["misc.total"] = new() { ["en"] = "Total", ["de"] = "Gesamt", ["fr"] = "Total", ["ja"] = "\u5408\u8a08" },
        ["misc.clickMap"] = new() { ["en"] = "Click to show on map", ["de"] = "Klick f\u00fcr Karte", ["fr"] = "Cliquer pour la carte", ["ja"] = "\u30af\u30ea\u30c3\u30af\u3067\u30de\u30c3\u30d7\u8868\u793a" },
        ["misc.step"] = new() { ["en"] = "Step", ["de"] = "Schritt", ["fr"] = "\u00c9tape", ["ja"] = "\u30b9\u30c6\u30c3\u30d7" },
        ["misc.search"] = new() { ["en"] = "Search...", ["de"] = "Suchen...", ["fr"] = "Chercher...", ["ja"] = "\u691c\u7d22..." },
        ["misc.close"] = new() { ["en"] = "Close", ["de"] = "Schlie\u00dfen", ["fr"] = "Fermer", ["ja"] = "\u9589\u3058\u308b" },
        ["misc.clear"] = new() { ["en"] = "Clear", ["de"] = "Leeren", ["fr"] = "Effacer", ["ja"] = "\u30af\u30ea\u30a2" },
        ["misc.resetFilter"] = new() { ["en"] = "Reset Filter", ["de"] = "Filter zur\u00fccksetzen", ["fr"] = "R\u00e9initialiser le filtre", ["ja"] = "\u30d5\u30a3\u30eb\u30bf\u30fc\u30ea\u30bb\u30c3\u30c8" },
        ["misc.markComplete"] = new() { ["en"] = "Mark as Completed", ["de"] = "Als erledigt markieren", ["fr"] = "Marquer termin\u00e9", ["ja"] = "\u5b8c\u4e86\u3068\u3057\u3066\u30de\u30fc\u30af" },
        ["misc.unmarkComplete"] = new() { ["en"] = "Unmark Completed", ["de"] = "Markierung aufheben", ["fr"] = "D\u00e9marquer", ["ja"] = "\u30de\u30fc\u30af\u89e3\u9664" },
        ["misc.noAether"] = new() { ["en"] = "No Aether Current quests found.", ["de"] = "Keine \u00c4therstrom-Quests gefunden.", ["fr"] = "Aucune qu\u00eate de courant \u00e9th\u00e9r\u00e9 trouv\u00e9e.", ["ja"] = "\u98a8\u8108\u30af\u30a8\u30b9\u30c8\u304c\u898b\u3064\u304b\u308a\u307e\u305b\u3093\u3002" },
        ["misc.noDuty"] = new() { ["en"] = "No content unlock quests found.", ["de"] = "Keine Inhaltsfreischaltungs-Quests gefunden.", ["fr"] = "Aucune qu\u00eate de d\u00e9blocage trouv\u00e9e.", ["ja"] = "\u30b3\u30f3\u30c6\u30f3\u30c4\u89e3\u653e\u30af\u30a8\u30b9\u30c8\u304c\u898b\u3064\u304b\u308a\u307e\u305b\u3093\u3002" },
        ["misc.noRecent"] = new() { ["en"] = "No recently viewed quests.", ["de"] = "Keine k\u00fcrzlich angesehenen Quests.", ["fr"] = "Aucune qu\u00eate vue r\u00e9cemment.", ["ja"] = "\u6700\u8fd1\u898b\u305f\u30af\u30a8\u30b9\u30c8\u306a\u3057\u3002" },
        ["misc.dutyDesc"] = new() { ["en"] = "All blue quests that unlock content. MSQ dungeons and Savage/individual Extreme fights (NPC dialog) are not listed.", ["de"] = "Alle blauen Quests die Inhalte freischalten. MSQ-Dungeons und Savage/einzelne Extreme-K\u00e4mpfe (NPC-Dialog) sind nicht aufgef\u00fchrt.", ["fr"] = "Toutes les qu\u00eates bleues qui d\u00e9bloquent du contenu. Les donjons MSQ et les combats Sadique/Extr\u00eame individuels ne sont pas list\u00e9s.", ["ja"] = "\u30b3\u30f3\u30c6\u30f3\u30c4\u3092\u89e3\u653e\u3059\u308b\u5168\u3066\u306e\u9752\u30af\u30a8\u30b9\u30c8\u3002MSQ\u30c0\u30f3\u30b8\u30e7\u30f3\u3084\u96f6\u5f0f/\u6975\u306e\u500b\u5225\u6226\u306f\u542b\u307e\u308c\u307e\u305b\u3093\u3002" },
        ["misc.missingReqs"] = new() { ["en"] = "Missing requirements:", ["de"] = "Fehlende Voraussetzungen:", ["fr"] = "Pr\u00e9requis manquants :", ["ja"] = "\u4e0d\u8db3\u3057\u3066\u3044\u308b\u8981\u4ef6:" },
        ["misc.planRoute"] = new() { ["en"] = "Plan Route", ["de"] = "Route planen", ["fr"] = "Planifier l'itin\u00e9raire", ["ja"] = "\u30eb\u30fc\u30c8\u8a08\u753b" },
        ["misc.sortRoute"] = new() { ["en"] = "Sort by optimal route (nearest zone first)", ["de"] = "Nach optimaler Route sortieren", ["fr"] = "Trier par route optimale", ["ja"] = "\u6700\u9069\u30eb\u30fc\u30c8\u3067\u4e26\u3073\u66ff\u3048" },
        ["misc.sortDistance"] = new() { ["en"] = "Sort by distance to player", ["de"] = "Nach Entfernung sortieren", ["fr"] = "Trier par distance", ["ja"] = "\u30d7\u30ec\u30a4\u30e4\u30fc\u304b\u3089\u306e\u8ddd\u96e2\u3067\u4e26\u3073\u66ff\u3048" },
        ["misc.selected"] = new() { ["en"] = "selected", ["de"] = "ausgew\u00e4hlt", ["fr"] = "s\u00e9lectionn\u00e9(s)", ["ja"] = "\u9078\u629e\u6e08\u307f" },

        // ── Settings Extra ──────────────────────────────────────────────
        ["settings.widgetBars"] = new() { ["en"] = "Widget Progress Bars", ["de"] = "Widget-Fortschrittsbalken", ["fr"] = "Barres de progression", ["ja"] = "\u30a6\u30a3\u30b8\u30a7\u30c3\u30c8\u30d7\u30ed\u30b0\u30ec\u30b9\u30d0\u30fc" },
        ["settings.data"] = new() { ["en"] = "Data", ["de"] = "Daten", ["fr"] = "Donn\u00e9es", ["ja"] = "\u30c7\u30fc\u30bf" },
        ["settings.sync"] = new() { ["en"] = "Sync with Game State", ["de"] = "Mit Spielstand synchronisieren", ["fr"] = "Synchroniser avec le jeu", ["ja"] = "\u30b2\u30fc\u30e0\u3068\u540c\u671f" },
        ["settings.manualOverrides"] = new() { ["en"] = "manual overrides", ["de"] = "manuelle \u00c4nderungen", ["fr"] = "modifications manuelles", ["ja"] = "\u624b\u52d5\u5909\u66f4" },
        ["settings.noOverrides"] = new() { ["en"] = "(no manual overrides)", ["de"] = "(keine manuellen \u00c4nderungen)", ["fr"] = "(aucune modification manuelle)", ["ja"] = "(\u624b\u52d5\u5909\u66f4\u306a\u3057)" },
        ["settings.expansions"] = new() { ["en"] = "Expansions", ["de"] = "Erweiterungen", ["fr"] = "Extensions", ["ja"] = "\u62e1\u5f35" },
        ["settings.widgetBarsCfg"] = new() { ["en"] = "Widget Bars", ["de"] = "Widget-Balken", ["fr"] = "Barres du widget", ["ja"] = "\u30a6\u30a3\u30b8\u30a7\u30c3\u30c8\u30d0\u30fc" },

        // ── Side Quest Filters ──────────────────────────────────────────
        ["filter.searchSide"] = new() { ["en"] = "Search side quests...", ["de"] = "Nebenquests suchen...", ["fr"] = "Chercher qu\u00eates annexes...", ["ja"] = "\u30b5\u30d6\u30af\u30a8\u30b9\u30c8\u691c\u7d22..." },

        // ── Recent Tab ──────────────────────────────────────────────────
        ["tab.recent"] = new() { ["en"] = "Recent", ["de"] = "Zuletzt", ["fr"] = "R\u00e9cents", ["ja"] = "\u6700\u8fd1" },
    };
}
