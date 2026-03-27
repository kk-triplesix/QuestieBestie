# QuestieBestie

**Never miss a blue quest again.** QuestieBestie is a Dalamud plugin for Final Fantasy XIV that tracks all blue (feature unlock) quests, shows their prerequisites, and helps you organize your quest journey with custom tracking lists and an in-game overlay.

---

## Features

### Blue Quest Database
- Browse **all blue (feature unlock) quests** in a sortable, searchable table
- Filter by **completion status**: All / Available / Incomplete / Complete
- **"Available" filter** shows only quests you can pick up right now (prerequisites met, not yet completed)
- Filter by **level range** and **text search** across all columns (name, level, class/job)
- **Hide class quests** checkbox to show only quests available to all classes
- Click any column header to **sort** ascending/descending

### Prerequisites & Requirements
- Click any quest to open a **detail window** with the full **recursive prerequisite tree**
- See prerequisites at every depth level — if Quest A requires Quest B, which requires Quest C, all are shown
- Each prerequisite displays its **completion status**, **type** (Blue / MSQ / Side), and is **clickable**
- Prerequisite chains **stop at MSQ quests** to keep the tree readable
- Level and class/job requirements shown for each quest

### Map Integration
- **Click any quest** in the table or detail window to open the **in-game map** with a flag at the quest giver's location
- Works for both blue quests and their prerequisites

### Custom Tracking Lists
- Create **multiple named lists** to organize your quests (e.g. "Dungeons", "Raids", "Crafting")
- **Right-click** any quest in the table to add it to an existing list or create a new one
- Lists are **saved persistently** across game restarts
- Switch between lists via the **dropdown in the header** or the **overlay tabs**

### In-Game Overlay
- Transparent, draggable **overlay window** that shows your active tracking list
- Toggle via the **"Show Overlay" button** in the main window or `/questie overlay`
- Shows quest names, level requirements, and **missing prerequisites with warnings**
- Click quests in the overlay to **open the map**
- Right-click to **remove quests** from the list
- **Switch between lists** directly in the overlay

### Overlay Customization
- Open **Settings** from the main window header
- Adjust **font scale**, **background opacity**, **border opacity**, and **window rounding**
- Customize **every color**: text, header, completed, level badge, warning, background, border
- **Reset to defaults** with one click
- All settings saved persistently

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
2. **Browse quests** using filters — the "Available" filter is selected by default, showing quests you can pick up right now
3. **Click a quest** to see it on the map and open the detail window with its full prerequisite chain
4. **Right-click a quest** to add it to a tracking list
5. **Show the overlay** to keep your tracked quests visible while playing
6. **Customize the overlay** appearance in Settings to match your UI preferences

---

## Installation

### Custom Repository
Add this URL in Dalamud under **Settings > Experimental > Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/kk-triplesix/QuestieBestie/repo/pluginmaster.json
```

Then search for **QuestieBestie** in the plugin installer.

### Manual / Development
1. Clone this repository
2. Build with `dotnet build`
3. The plugin is automatically copied to your `devPlugins` folder

---

## Tech Stack

- .NET 10 / C# latest
- Dalamud.NET.Sdk 14
- ECommons
- ImGui (via Dalamud.Bindings.ImGui)
- Lumina (FFXIV game data)
- FFXIVClientStructs (game memory access)

---

## License

This project is provided as-is for personal use with FFXIV.
