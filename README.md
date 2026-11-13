# WeaponConfigStats (SPT 4.1.6)

Client BepInEx mod for weapon configurator tools:

- **Green area:** per-stat toggle + sort buttons beside `CharacteristicsPanel`
- **Orange area:** selected-stat overlays on each `ModdingSelectableItemView` in the attachment dropdown
- **Sorting:** open attachment list reorders by the active sort stat (Asc/Desc); switching stats resets the previous column
- **F12 config:** enabled stats, sort mode, delta vs absolute, overlay font size

## Status

`v0.1.10`:

- Harmony hooks compile and load
- Toolbar controls and tile overlays are live
- `AttachmentStatResolver` reads Mod properties / item attributes
- Live dropdown reordering via `AttachmentListSorter.ApplyToOpenMenu`

## Build

```powershell
cd G:\Games\SPT\Development\WeaponConfigStats
dotnet build -c Release
```

Output is auto-copied to:

`G:\Games\SPT\BepInEx\plugins\WeaponConfigStats\WeaponConfigStats.dll`

## Project layout

| Path | Role |
|------|------|
| `Plugin.cs` | BepInEx entry |
| `Config/ModConfig.cs` | F12 settings |
| `Models/` | `WeaponStatId`, sort enums |
| `Services/` | selection, resolver, sorter |
| `UI/` | toolbar + tile overlay |
| `Patches/` | `CharacteristicsPanel.*`, `DropDownMenu.Show/Create/Close` |

## Next implementation steps

1. Polish layout for RU/EN UI and trader modding screen
2. Sort button visual polish beyond `A` / `D` / `-`
