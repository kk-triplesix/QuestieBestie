<p align="center">
  <img src="QuestieBestieLogo.png" alt="QuestieBestie Logo" width="400">
</p>

<h1 align="center">QuestieBestie</h1>
<p align="center"><b>The ultimate blue quest companion for FFXIV.</b><br>Track every unlock, plan your route, never miss a feature.</p>

---

## Features

### Blue Quest Database
- Complete list of all blue (feature unlock) quests
- Sortable table with auto-fit columns
- Color-coded expansion tags: ARR, HW, SB, ShB, EW, DT
- Category icons for Dungeons, Trials, Raids, Jobs, and Features
- "NEW" badge on quests added after a game patch
- Quest chains visually grouped with indentation
- Multi-select with Ctrl+Click to bulk-add quests to tracking lists
- Mark quests as completed directly in the table

### Smart Filters
- Filter by completion status: All / Available / Incomplete / Complete
- Filter by expansion, class/job, location, quest type, and unlock
- All filter dropdowns include built-in search
- Level range filter and full-text search across all columns
- "Nearest" button to sort by distance to your character

### 6 Tabs
- **Quests** — Full blue quest database with all filters
- **Aether Currents** — Progress bars per zone with expansion colors
- **Duty Unlocks** — Dungeons, Trials, and Raids grouped by category with progress bars
- **Side Quests** — Yellow quests with special highlighting for emote rewards, beast tribes, and blue quest prerequisites
- **Recent** — Last 20 viewed quests
- **Statistics** — Completion progress per expansion, per type, and overall with celebration at 100%

### Quest Details
- Full recursive prerequisite tree (stops at MSQ quests)
- MSQ requirement indicator with completion status
- Quest giver NPC name, Gil and EXP rewards
- Favorite button and personal notes field (up to 512 characters)
- Click any prerequisite to show it on the map

### Unlock Descriptions
- Shows what each quest unlocks: dungeons, trials, raids, jobs, glamour, hunts, beast tribes, Gold Saucer, housing, PvP, deep dungeons, and 200+ more
- Chain propagation: if a quest chain leads to a dungeon, all quests in the chain show what you're working towards

### Custom Tracking Lists
- Create unlimited named tracking lists
- Right-click any quest to add it to a list
- Rename, delete, export and import lists via clipboard
- Auto-remove completed quests (optional)
- Undo after accidental removal

### In-Game Overlay
- Transparent, resizable floating window with your active tracking list
- Plan Route button for optimal quest order (nearest zone first)
- Warning icons when prerequisites are not yet met
- Settings and close buttons appear on hover
- Switch between lists directly in the overlay
- Right-click quests to mark complete or remove

### Progress Widget
- Compact mini-overlay with progress bars
- Configurable: show total or per expansion
- Direction arrow pointing to nearest tracked quest with distance
- Individual expansion bars can be toggled on/off

### Manual Completion & Game Sync
- Mark any quest as completed manually, even if the game doesn't track it
- Sync with Game State button to reset all manual overrides back to the actual game status

### Customization
- Font scale, background and border opacity, window rounding
- 7 fully customizable colors
- Auto-remove completed quests from overlay
- Chat notifications when new quests become available
- Sound notifications

### Map & Chat Integration
- Click any quest anywhere to open the in-game map with a flag on the quest giver
- "Send to Chat" shares quest info as a clickable map link
- Chat notifications when new quests become available
- Detail window opens when quest names appear in chat

### Favorites & Notes
- Star system for favorite quests (always sorted to top)
- Personal notes per quest, saved persistently
- "NEW" badge on quests added since your last login

### Multi-Language Support
- Full UI localization for English, German, French, and Japanese
- Automatically detects your client language
- Quest names and NPC names follow the game's localization

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
| `/questie search <name>` | Search for a quest and open details + map |

---

## How It Works

1. Open the main window with `/questie` or click the DTR bar entry
2. Browse quests using filters — "Available" shows quests you can pick up right now
3. Click a quest to see it on the map and view the prerequisite chain
4. Right-click to add to a tracking list, favorite, or send to chat
5. Show the overlay to keep tracked quests visible while playing
6. Use Plan Route for optimal quest order
7. Check Statistics for your completion progress
8. Customize everything in Settings

---

## Installation

Add this URL in Dalamud under **Settings > Experimental > Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/kk-triplesix/QuestieBestie/repo/pluginmaster.json
```

Then search for **QuestieBestie** in the plugin installer.

---

## Contact

- Discord: **666kk_**
- GitHub: [kk-triplesix](https://github.com/kk-triplesix)

---

## AI Disclaimer

This plugin was developed with the assistance of AI tools. The plugin icon was also generated using AI. All code has been reviewed, tested, and validated by the developer.

---

## License

This project is provided as-is for personal use with FFXIV.
