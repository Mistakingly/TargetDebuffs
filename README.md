# Target Debuffs

A Dalamud plugin for FFXIV that shows the DoTs and debuffs you apply to your target in their own draggable, lockable overlay, and can hide them from the game's target bar.

[![Last commit](https://img.shields.io/github/last-commit/Mistakingly/TargetDebuffs)](https://github.com/Mistakingly/TargetDebuffs/commits)
[![Issues](https://img.shields.io/github/issues/Mistakingly/TargetDebuffs)](https://github.com/Mistakingly/TargetDebuffs/issues)
[![Latest release](https://img.shields.io/github/v/release/Mistakingly/TargetDebuffs)](https://github.com/Mistakingly/TargetDebuffs/releases/latest)

- [About](#about)
- [Features](#features)
- [Compatibility](#compatibility)
- [Installation](#installation)
- [Commands](#commands)
- [Settings](#settings)
- [Known Limitations](#known-limitations)
- [Contributing](#contributing)

## About

Your damage-over-time effects are easy to lose in a crowded target bar, especially on a boss that carries a lot of party debuffs. Target Debuffs pulls out only the statuses you applied, such as a Sage's Eukrasian Dosis or a Dragoon's Chaotic Spring, and puts them in a separate overlay that you can place anywhere on screen.

It was made for DoT tracking, so by default it shows DoTs only. Everything else it can show (all of your debuffs, your pet's statuses) is opt-in.

## Features

### Separate status overlay

- Shows the statuses you applied to your current target, with the game's own icons.
- Remaining time is drawn on each icon, and stack counts on stackable statuses.
- Drag the overlay anywhere while it is unlocked. Its position is remembered between sessions.
- Lock it to make it click-through, with no background, so it never gets in the way of the game.

### DoT filtering

- "Only show DoTs" is on by default. Only the DoTs for the jobs you tick in settings are shown.
- Covered jobs: WHM, SCH, AST, SGE, DRG, RPR, MNK, SAM, BRD, MCH, BLM, SMN, PLD and GNB.
- GNB is off by default, since its DoTs are not worth tracking. Tick it if you want it.
- Hover over a job's tick box to see which statuses it covers.
- Missing a DoT, or want to track a non-DoT debuff? Add its exact in-game status name under "Extra status names".
- Switch the filter off to show every debuff you have applied instead, such as Feint or Addle.
- Optionally include statuses applied by your pet or summon.

### Bosses only

- Optionally show the overlay only while your target is a boss.
- Striking Dummies always count as bosses.
- Other targets count as a boss when they are an NPC with at least a set amount of max HP (3,000,000 by default). The threshold is adjustable, and settings show your current target's name, max HP and whether it counts as a boss, so you can tune it.

### Hide statuses on the game's target bar

- Optionally hides the statuses shown in the overlay from the game's own target status bar, so they are not shown twice.
- Only the statuses the overlay is showing are hidden, and only ones you applied yourself. Other players' debuffs and your own statuses that are not on the overlay stay where they are.
- Works with both the split target frame (HP and Status as separate elements) and the merged target frame.
- "Close the gaps they leave" slides the remaining icons left to fill the space.
- Everything is restored when the option is turned off or the plugin is unloaded.

### Appearance

- Text size, text color and text outline.
- Icon scale, icons per row and spacing.

## Compatibility

### BigPlayerDebuffs

[BigPlayerDebuffs](https://github.com/rgd87/BigPlayerDebuffs) enlarges your own statuses on the target bar by moving and scaling the same icons that Target Debuffs hides. Running both works, but they would fight over icon positions, so Target Debuffs does not close gaps while BigPlayerDebuffs is loaded. If you hide your DoTs with Target Debuffs, you can turn BigPlayerDebuffs off and let the gaps close.

## Installation

1. Open Dalamud settings with `/xlsettings` and go to the **Experimental** tab.
2. Under **Custom Plugin Repositories**, paste the repository URL into an empty row, tick it, then click **Save**:

   ```
   https://raw.githubusercontent.com/Mistakingly/TargetDebuffs/main/repo.json
   ```

3. Open the plugin installer with `/xlplugins`.
4. Search for **Target Debuffs** and install it.
5. Type `/tdebuffs` to open the settings window.

## Commands

| Command | Function |
| --- | --- |
| `/tdebuffs` | Opens or closes the settings window. |
| `/tdebuffs lock` | Locks the overlay in place and makes it click-through. |
| `/tdebuffs unlock` | Unlocks the overlay so it can be dragged. |
| `/tdebuffs toggle` | Switches between locked and unlocked. |

## Settings

Open them with `/tdebuffs`, or from the plugin installer's settings button.

| Setting | What it does |
| --- | --- |
| Lock overlay | Fixes the overlay in place and makes it click-through. |
| Only statuses I applied | Shows only statuses you applied. |
| Include statuses from my pet / summon | Also counts statuses applied by your pet or summon. |
| Only show DoTs | Limits the overlay to the DoT list and your extra names. |
| DoT jobs | Which jobs' DoTs to show. |
| Extra status names | Exact in-game status names to always include. |
| Only show on bosses | Shows the overlay only on bosses and Striking Dummies. |
| Boss min max-HP | Max HP an NPC needs to count as a boss. |
| Hide these statuses on the game's target bar | Hides the overlay's statuses from the game's own bar. |
| Close the gaps they leave | Moves remaining icons left to fill hidden slots. |
| Show timers / stack counts | Toggles the text drawn on each icon. |
| Text size, color and outline | Styling for the timer and stack text. |
| Icon scale, icons per row, spacing | Layout of the overlay. |

## Known Limitations

- DoT names are matched in English. On a client in another language, add the local names under "Extra status names".
- There is no flag the plugin can read to tell whether a target is a boss. The boss check uses the name (for dummies) and max HP, so some enemies may be misjudged. Adjust the max-HP threshold if so.
- The target bar hiding relies on how the game orders and lays out its status icons. A game patch that changes the target bar could break it. If an icon disappears from or stays on the bar when it should not, please open an issue.

## Contributing

Bug reports and suggestions are welcome through [issues](https://github.com/Mistakingly/TargetDebuffs/issues). If you report a bug, please include which target frame you use (split or merged), which other plugins touch your target bar, and what you expected to see.
