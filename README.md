<p align="center">
  <img src="QuestieBestieLogo.png" alt="QuestieBestie Logo" width="400">
</p>

<h1 align="center">QuestieBestie</h1>
<p align="center"><b>The ultimate blue quest companion for FFXIV.</b><br>Track every unlock, plan your route, never miss a feature.</p>

---

## Features

### Quest Database
- All blue (feature unlock) quests in a sortable table with auto-fit columns
- Color-coded expansion tags: ARR, HW, SB, ShB, EW, DT
- Category icons for Dungeons, Trials, Raids, Jobs, and Features
- "NEW" badge on quests added after a game patch
- Quest chains visually grouped with indentation
- Multi-select to bulk-add quests to tracking lists

### Smart Filters
- Filter by completion status: All / Available / Incomplete / Complete
- Filter by expansion, class/job, location, quest type, and unlock — all with built-in search
- Level range filter and full-text search across all columns
- "Nearest" button to sort by distance to your character

### 6 Tabs
- **Quests** — Full blue quest database with all filters
- **Aether Currents** — Progress bars per zone with expansion colors
- **Duty Unlocks** — Dungeons, Trials, Raids, and other content unlocks
- **Side Quests** — Yellow quests with special highlighting (blue quest prerequisites, emotes, beast tribes)
- **Recent** — Last 20 viewed quests
- **Statistics** — Progress per expansion, type, and quest chain with completion celebration

### Quest Details
- Full recursive prerequisite tree (stops at MSQ quests)
- MSQ requirement indicator
- Quest giver NPC name and Gil rewards
- Quest chain progress tracking
- Favorite button and personal notes field

### Unlock Descriptions
- Shows what each quest unlocks: dungeons, trials, raids, jobs, glamour, hunts, beast tribes, Gold Saucer, housing, PvP, deep dungeons, and 200+ more
- Chain propagation: if a quest chain leads to a dungeon, all quests in the chain show the result

### Custom Tracking Lists
- Create multiple named lists
- Right-click any quest to add, rename, delete, export/import via clipboard
- Auto-remove completed quests (optional)
- Undo after accidental removal

### In-Game Overlay
- Transparent, resizable overlay with your active tracking list
- Plan Route button for optimal quest order (nearest zone first)
- Settings and close buttons on hover
- Switch between lists directly in the overlay

### Progress Widget
- Compact mini-overlay with progress bar and direction arrow
- Configurable: show total, per expansion, or per quest chain
- Direction arrow pointing to nearest tracked quest with distance

### Overlay Customization
- Font scale, background/border opacity, window rounding
- 7 customizable colors
- Auto-remove, chat notifications, sound notifications
- Widget bars configurable per expansion and chain

### Map & Chat Integration
- Click any quest anywhere to open the in-game map with a flag
- "Send to Chat" shares quest info as a map link
- Chat notifications when new quests become available
- Quest journal hook: detail window opens when clicking quest links in chat

### Favorites, Notes & "What's New"
- Star system for favorite quests (always sorted to top)
- Personal notes per quest
- "NEW" badge on quests added since your last login
- All saved persistently

### Multi-Language Support
- Full UI localization for English, German, French, and Japanese
- Automatically detects your client language
- Quest names and NPC names use the game's localization

### Server Info Bar
- Shows completion percentage in the DTR bar
- Click to toggle the main window

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

1. Open the main window with `/questie` or click the DTR bar entry
2. Browse quests using filters — "Available" shows quests you can pick up right now
3. Click a quest to see it on the map and view the prerequisite chain
4. Right-click to add to a tracking list, favorite, or send to chat
5. Show the overlay to keep tracked quests visible while playing
6. Use Plan Route for optimal quest order
7. Check Statistics for completion progress
8. Customize everything in Settings

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
