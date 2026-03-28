<p align="center">
  <img src="QuestieBestieLogo.png" alt="QuestieBestie Logo" width="400">
</p>

<h1 align="center">QuestieBestie</h1>
<p align="center"><b>The ultimate blue quest companion for FFXIV.</b><br>Track all feature unlock quests, see what they unlock, plan your route, and never miss a dungeon, trial, or feature again.</p>

---

## Features

### Blue Quest Database
- Browse **all blue (feature unlock) quests** in a 7-column sortable table
- Columns: Favorite (★), Quest Name, Level, Expansion, Location, Class/Job, Unlocks
- **Category icons** next to each quest: ⚔ Dungeon, ✦ Trial, ♕ Raid, ☆ Job, ⚙ Feature
- **Color-coded expansion tags**: ARR (blue), HW (cyan), SB (red), ShB (purple), EW (gold), DT (green)
- **"NEW" badge** on quests added since your last login
- **Multi-select**: Ctrl+Click to select multiple quests and add all to a tracking list at once

### Smart Filters
- Filter by **completion status**: All / Available / Incomplete / Complete
- **"Available"** shows only quests you can pick up right now (all prerequisites met)
- Filter by **expansion**, **class/job**, **location**, and **quest type** — all with built-in search fields
- Filter by **level range** with min/max inputs
- **Full-text search** across all columns (name, level, location, class/job, expansion, unlocks)
- **"Nearest" button** to sort quests by distance to your character

### Favorites, Notes & Recent
- **Star system** to mark favorite quests — favorites always sorted to top
- **Personal notes** per quest (visible in detail window and tooltips, saved persistently)
- **Recent tab** showing your last 20 viewed quests

### Unlock Descriptions
- Shows **what each quest unlocks**: dungeons, trials, raids, jobs, glamour, dye, materia melding, desynthesis, treasure maps, emotes, beast tribes, Aether Currents, and more
- Detected via game data: InstanceContentUnlock, ClassJobUnlock, GeneralActionReward, EmoteReward, OtherReward

### Rich Tooltips
- Hover over any quest for **all info at a glance**:
- Expansion (color-coded), Level, Location, Quest Giver NPC, Class/Job, Type with icon, Unlocks, Rewards (Gil), Prerequisites, Chain progress, Notes

### Quest Details & Prerequisites
- Click any quest for a **detail window** with the full **recursive prerequisite tree**
- Prerequisites shown at every depth, **stopping at MSQ quests**
- **MSQ requirement indicator** showing which Main Scenario Quest you need to complete
- Each prerequisite is **clickable** (opens map)
- **Quest chains** automatically detected with progress tracking (Step X, Y/Z done)
- **Quest Giver NPC name** displayed
- **Gil rewards** shown
- **Favorite button** and **notes field** in detail window

### Map & Chat Integration
- **Click any quest** anywhere in the plugin to open the in-game map with a flag at the quest giver
- **"Send to Chat"** via right-click to share a quest location as a map link
- Works for blue quests, side quests, and prerequisites

### Route Planner
- **Optimized quest route** for your tracking list
- Groups quests by zone, then sorts by nearest-neighbor within each zone
- Current zone prioritized first

### Custom Tracking Lists
- Create **multiple named lists** (e.g. "Dungeons", "Raids", "Crafting")
- **Right-click** any quest to add to a list or create a new one
- **Rename and delete** lists via the edit button in the header
- **Export/Import** lists via clipboard (JSON) to share with friends
- **Auto-remove** completed quests from lists (optional)
- **Undo** button after accidentally removing a quest
- **Multi-select** to add many quests at once (Ctrl+Click)

### In-Game Overlay
- Transparent, resizable, draggable overlay with your active tracking list
- **Settings gear (⚙)** and **close button (✕)** appear on hover
- **Background opacity increases on hover** for readability
- Click quests to open map, right-click to remove
- **Switch between lists** directly in the overlay

### Progress Widget
- Compact **mini-overlay** showing overall completion percentage
- **Direction arrow** (↑↓←→) pointing to nearest tracked quest in your zone
- Distance in yalms displayed

### Overlay Customization
- Adjust **font scale**, **background opacity**, **border opacity**, **window rounding**
- Customize **every color**: text, header, completed, level badge, warning, background, border
- **Reset to defaults** button
- All settings saved persistently

### Aether Currents Tab
- All Aether Current quests **grouped by zone**
- **Progress bars** per zone with expansion colors
- Click any quest to open map

### Duty Unlock Checklist
- All **dungeons, trials, and raids** grouped by type
- **Expansion tags** and unlock descriptions
- Quick overview of what duties you still need to unlock

### Side Quests Tab
- Browse **all yellow side quests**
- **Special highlighting** (gold) for quests that:
  - Are prerequisites for blue quests ("Required for: [Quest Name]")
  - Give emote rewards
  - Unlock general actions (companions, etc.)
  - Belong to beast tribes
- Filter: All / Special Only / Incomplete / Complete + Expansion filter

### Statistics Tab
- **Overall progress** bar
- **Per-expansion** progress bars with expansion colors
- **Per-type** progress bars (Feature, Job Unlock, Dungeon, Trial, Raid)
- **Quest chain** progress bars with completion tracking
- **✨ COMPLETE! ✨** celebration when a category reaches 100%

### Quest Journal Integration
- When you **click a quest link in chat**, QuestieBestie automatically opens its detail window
- Works seamlessly alongside the normal quest journal
- Only triggers for blue quests

### Notifications
- **Chat notifications** when a new blue quest becomes available (prerequisites met)
- Colored messages in the Echo channel
- Toggleable in Settings

### Multi-Language Support
- Full UI localization for **English, German, French, and Japanese**
- Automatically detects your FFXIV client language
- Quest names use the game's built-in localization

### Server Info Bar (DTR)
- Shows **completion percentage** (`QB 67%`) in the server info bar
- **Click** to toggle the main window

---

## Commands

| Command | Description |
|---------|-------------|
| `/questie` | Toggle the main window |
| `/questie overlay` | Toggle the in-game overlay |
| `/questie widget` | Toggle the progress widget |
| `/questie stats` | Open the main window |
| `/questie search <name>` | Search for a quest and open details + map |

---

## How It Works

1. **Open the main window** with `/questie` or click the DTR bar entry
2. **Browse quests** using filters — "Available" is the default
3. **Click a quest** to see it on the map and view the prerequisite chain
4. **Right-click a quest** to add to a tracking list, favorite it, or send to chat
5. **Ctrl+Click** to select multiple quests and bulk-add to a list
6. **Show the overlay** to keep tracked quests visible while playing
7. **Enable the widget** for a compact progress bar with direction arrow
8. **Check the Statistics tab** for completion progress per expansion, type, and chain
9. **Use the Aether Currents tab** to track flight unlock progress
10. **Use the Duty Unlocks tab** for a dungeon/trial/raid checklist
11. **Browse Side Quests** to find important yellow quests (blue quest prerequisites, emotes)
12. **Share lists** with friends via Export/Import
13. **Customize everything** in Settings

---

## Installation

Add this URL in Dalamud under **Settings > Experimental > Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/kk-triplesix/QuestieBestie/repo/pluginmaster.json
```

Then search for **QuestieBestie** in the plugin installer.

---

## License

This project is provided as-is for personal use with FFXIV.
