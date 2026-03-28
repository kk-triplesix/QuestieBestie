# QuestieBestie

**Never miss a blue quest again.** QuestieBestie is a Dalamud plugin for Final Fantasy XIV that tracks all blue (feature unlock) quests, shows their prerequisites, and helps you organize your quest journey with custom tracking lists and an in-game overlay.

---

## Features

### Blue Quest Database
- Browse **all blue (feature unlock) quests** in a sortable, searchable table
- **6 columns**: Status, Quest Name, Level, Location, Class/Job, Unlocks
- Filter by **completion status**: All / Available / Incomplete / Complete
- Filter by **expansion**: A Realm Reborn, Heavensward, Stormblood, Shadowbringers, Endwalker, Dawntrail
- Filter by **class/job**, **location**, and **quest type** (Feature, Job Unlock, Dungeon, Trial, Raid)
- All filter dropdowns have a **built-in search field** for quick selection
- Filter by **level range** with min/max inputs
- **Text search** across all columns (name, level, location, class/job, expansion, unlocks)
- **"Nearest" button** to sort quests by distance to your current position
- Click any column header to **sort** ascending/descending

### Quest Details & Prerequisites
- Click any quest to open a **detail window** with the full **recursive prerequisite tree**
- Prerequisites shown at every depth level (Quest A > Quest B > Quest C)
- Each prerequisite displays **completion status**, **type** (Blue / MSQ / Side), and is **clickable**
- Prerequisite chains **stop at MSQ quests** to keep the tree readable
- **Quest chains** are automatically detected and shown in tooltips

### Unlock Descriptions
- Each quest shows **what it unlocks**: dungeons, trials, raids, jobs, features, emotes, actions, beast tribes
- Detected via game data: InstanceContentUnlock, ClassJobUnlock, GeneralActionReward, EmoteReward, OtherReward
- Covers glamour, dye, materia melding, desynthesis, treasure maps, Aether Currents, and more

### Rich Tooltips
- Hover over any quest for a **detailed tooltip** showing:
- Expansion, Level, Location, Class/Job, Quest Type, Unlocks, Prerequisites, Chain info

### Map Integration
- **Click any quest** in the table, detail window, or overlay to open the **in-game map** with a flag at the quest giver
- Works for both blue quests and their prerequisites

### Custom Tracking Lists
- Create **multiple named lists** to organize your quests (e.g. "Dungeons", "Raids", "Crafting")
- **Right-click** any quest in the table to add it to an existing list or create a new one
- **Export/Import** lists via clipboard (JSON) to share with friends
- Lists are **saved persistently** across game restarts
- **Auto-remove** completed quests from lists (optional, in Settings)

### In-Game Overlay
- Transparent, resizable, draggable **overlay window** with your active tracking list
- Toggle via **"Show Overlay" button** or `/questie overlay`
- Quest names, level requirements, and **missing prerequisites with warnings**
- Click quests to **open the map**, right-click to **remove from list**
- **Settings gear** and **close button** appear on hover
- **Switch between lists** directly in the overlay
- **Background opacity increases on hover** for readability

### Overlay Customization
- Adjust **font scale**, **background opacity**, **border opacity**, and **window rounding**
- Customize **every color**: text, header, completed, level badge, warning, background, border
- **Reset to defaults** with one click
- All settings saved persistently

### Statistics Tab
- **Overall progress** bar with completion percentage
- **Per-expansion** progress bars (ARR, HW, SB, ShB, EW, DT)
- **Per-type** progress bars (Feature, Job Unlock, Dungeon, Trial, Raid)

### Notifications
- **Chat notifications** when a new blue quest becomes available (prerequisites met)
- Colored messages in the Echo channel
- Can be toggled in Settings

### Server Info Bar (DTR)
- Shows **completion percentage** (`QB 67%`) in the Dalamud server info bar
- **Click** to toggle the main window

---

## Commands

| Command | Description |
|---------|-------------|
| `/questie` | Toggle the main window |
| `/questie overlay` | Toggle the in-game overlay |

---

## How It Works

1. **Open the main window** with `/questie` or click the DTR bar entry
2. **Browse quests** using filters — "Available" is the default, showing quests you can pick up right now
3. **Click a quest** to see it on the map and view the full prerequisite chain
4. **Right-click a quest** to add it to a tracking list
5. **Show the overlay** to keep your tracked quests visible while playing
6. **Check the Statistics tab** to see your completion progress per expansion and type
7. **Customize** the overlay appearance and behavior in Settings
8. **Share lists** with friends using Export/Import via clipboard

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
