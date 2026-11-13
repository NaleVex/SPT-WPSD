# WeaponConfigStats (SPT 4.1.6)

Client BepInEx scaffold for weapon configurator tools:

- **Green area:** per-stat toggle + sort buttons beside `CharacteristicsPanel`
- **Orange area:** selected-stat overlays on each `ModdingSelectableItemView` in the attachment dropdown
- **F12 config:** enabled stats, sort mode, delta vs absolute, overlay font size

## Status

Scaffold only (`v0.1.0`):

- Harmony hooks compile and load
- Toolbar / overlay UI shells are created
- Stat numbers are **stubs** (`0`) until `AttachmentStatResolver` is implemented
- List reordering is not applied to the live dropdown yet (`AttachmentListSorter` is ready)

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
| `Services/` | selection, resolver stub, sorter |
| `UI/` | toolbar + tile overlay |
| `Patches/` | `CharacteristicsPanel.Show`, `DropDownMenu.Show/Create/Close` |

## Next implementation steps

1. Map `WeaponStatId` → `EItemAttributeId` / `ItemAttribute`
2. Resolve installed slot item from `ModdingScreenSlotView`
3. Fill `AttachmentStatResolver` (part attributes or weapon-clone delta)
4. Anchor `StatRowControls` to real `CompactCharacteristicPanel` rows
5. Apply `AttachmentListSorter` to dropdown children / `_itemsCollection`
6. Polish layout for RU/EN UI and trader modding screen
