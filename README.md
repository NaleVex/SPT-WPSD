# Weapon Parts Stats Display (WPSD)

Client BepInEx mod for **SPT 4.1.6** that shows attachment stats in the weapon configurator and lets you sort the attachment list by those stats.

| | |
|--|--|
| **Author** | FunApple |
| **Plugin name** | `FunApple-WeaponPartsStatsDisplay` |
| **GUID** | `com.funapple.wpsd` |
| **Version** | `1.0.1` |
| **Type** | Client only |
| **Dependencies** | SPT (`com.SPT.core` / BepInEx) |
| **License** | [MIT](LICENSE) |
| **Source** | https://github.com/NaleVex/SPT-WPSD |

## Features

- Toggle which stats appear on attachment tiles (toolbar beside the characteristics panel)
- Sort the open attachment dropdown by the selected stat (Asc / Desc)
- Overlay values on each attachment option (and absolute contribution on equipped slot tiles)
- Accuracy, muzzle velocity, and recoil overlays show as percentages
- F12 config for enabled stats, sort mode, delta vs absolute, overlay font size / color / decimals

Default overlay stats: Ergonomics, Vertical Recoil, Horizontal Recoil.

Toolbar toggles are not shown for durability, fire rate, or effective distance.

## Installation

1. Download the latest release `.zip` from [GitHub Releases](https://github.com/NaleVex/SPT-WPSD/releases).
2. Extract it into your SPT root folder (the folder that contains `BepInEx` and `EscapeFromTarkov.exe`).
3. Confirm this path exists after extract:

   `BepInEx/plugins/WPSD/WPSD.dll`

4. Launch the game. Open F12 → **FunApple-WeaponPartsStatsDisplay** to configure.

## Usage

1. Open the weapon configurator (modding screen).
2. Use the toolbar next to the characteristics panel to enable stats and choose sort direction.
3. Open an attachment slot dropdown — overlays appear on each option; sorting reorders the list when enabled.
4. Adjust display options in F12 as needed.

### Useful F12 settings

| Setting | Purpose |
|--------|---------|
| Enabled | Master on/off |
| ShowDeltasVsInstalled | Dropdown tiles show delta vs installed part when true |
| EnabledStats | Comma-separated stats to show on tiles |
| OverlayFontSize / OverlayColor / OverlayDecimalPlaces | Overlay appearance |

## Uninstall

Delete `BepInEx/plugins/WPSD/` and optionally `BepInEx/config/com.funapple.wpsd.cfg`.

## Build (developers)

Requires a local SPT install next to this repo (or set `SptDir` when building). The `.csproj` defaults to two folders up from this project → SPT root.

```powershell
dotnet build -c Release
```

Release output is copied to:

`BepInEx/plugins/WPSD/WPSD.dll`

## Project layout

| Path | Role |
|------|------|
| `Plugin.cs` | BepInEx entry (`FunApple-WeaponPartsStatsDisplay` / `1.0.1`) |
| `Config/ModConfig.cs` | F12 settings |
| `Models/` | Stat IDs, sort enums |
| `Services/` | Selection, resolver, sorter |
| `UI/` | Toolbar + tile overlay |
| `Patches/` | Characteristics panel + dropdown hooks |

## Changelog

### 1.0.1

- Author / Forge nickname: **FunApple**
- Forge-compliant plugin name: `FunApple-WeaponPartsStatsDisplay`
- Assembly / plugin folder / GUID use **WPSD** (`com.funapple.wpsd`, `BepInEx/plugins/WPSD/WPSD.dll`)
- Removed toolbar toggles for durability, fire rate, and effective distance
- Accuracy and muzzle velocity overlays format as percentages

### 1.0.0

- First public release for SPT 4.1.6
- Attachment overlays, characteristics toolbar toggles/sort, live dropdown sorting
- Recoil overlays use weapon force conversion so VRec/HRec match the characteristics panel
