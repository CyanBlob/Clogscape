# Bountyscape

A tile-based progression tracker for Old School RuneScape. The game board is a grid of tiles representing skills, quests, and achievement diaries. Tiles start hidden — completing bounties earns keys, keys unlock adjacent tiles, and claiming an unlocked tile reveals its neighbours, gradually expanding your visible map.

## Requirements

- [Godot 4.7 (Mono/.NET edition)](https://godotengine.org/download/)
- [.NET SDK 8+](https://dotnet.microsoft.com/download)

## Running the game

1. Open Godot and import the project (`project.godot`)
2. Press **F5** or click the **Play** button

## Building releases

`build.sh` exports the game for macOS, Windows, and Linux using Godot's headless CLI.

**First-time setup:**
1. In the Godot editor, go to **Editor → Export → Manage Export Templates** and download the official templates
2. For Windows and Linux, go to **Project → Export → Add** and add "Windows Desktop" and "Linux/X11" presets (macOS is already configured)

**Running the build script:**
```bash
./build.sh              # all platforms
./build.sh mac          # just macOS
./build.sh mac windows  # specific platforms
```

Builds are output to `../bountyscape_builds/{mac,windows,linux}/`.

If Godot isn't at the default location (`/Applications/Godot_mono.app`), set `GODOT_PATH`:
```bash
GODOT_PATH=/path/to/Godot ./build.sh
```

## Project structure

```
Scripts/          C# game logic
  Game.cs         Save/load, game state, bounty rolling
  Tile.cs         Tile interaction and reveal logic
  TileGenerator.cs  Grid generation and layout
  UI.cs           HUD and player management
  Skill.cs        Skill tile definitions and unlock rules
  Quest.cs        Quest tile definitions
  Diary.cs        Achievement diary tile definitions
  Bounty.cs       Bounty data model
  Unlockable.cs   Base class for all tile types

default_possible_bounties.json   The bounties pool (see below)
export_presets.cfg               Godot export configuration
build.sh                         Release build script
```

## Editing the bounties list

Bounties live in `default_possible_bounties.json`. Each entry is a JSON object:

```json
{
    "name": "Example Bounty",
    "description": "Do something cool",
    "difficulty": 2,
    "bountyType": 0,
    "extraKeyChance": 25,
    "skipChance": 0,
    "skipChancePerCompletion": 30,
    "maxLifetimeKeys": 3,
    "countMin": null,
    "countMax": null,
    "lastCompleted": null,
    "lastRolled": null,
    "isWildy": false,
    "requirementLocked": false,
    "completedLocked": false
}
```

**`difficulty`** — controls key rewards and when the bounty appears:

| Value | Tier |
|---|---|
| 0 | Novice |
| 1 | Easy |
| 2 | Medium |
| 3 | Hard |
| 4 | Expert |
| 5 | Grandmaster |

**`bountyType`** — category of activity:

| Value | Type |
|---|---|
| 0 | Boss |
| 1 | Raid |
| 2 | Clue |
| 3 | Minigame |
| 4 | Grind |
| 5 | Skilling |
| 6 | Challenge |
| 7 | Misc |
| 8 | Fetch |
| 9 | Real Life |
| 10 | Slayer |

**Key fields:**

- `skipChance` — percentage chance this bounty is skipped when rolled (0–100)
- `skipChancePerCompletion` — how much `skipChance` increases each time it's completed; set to 100 to retire it after one completion
- `maxLifetimeKeys` — cap on total keys this bounty can ever award across all completions; omit for no cap
- `extraKeyChance` — percentage chance of an extra key beyond the default for the difficulty
- `countMin` / `countMax` — if the description uses `{{COUNT}}`, a random value in this range is substituted at display time
- `completedLocked` — set to `true` to permanently disable a bounty (it will always be re-rolled)

Minigames or content with quest requirements should have their difficulty set at least one tier higher than the quest that gates them (e.g. content requiring an Intermediate quest should be difficulty 3+).
