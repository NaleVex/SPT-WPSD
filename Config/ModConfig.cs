using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using WeaponConfigStats.Models;
using WeaponConfigStats.Services;

namespace WeaponConfigStats.Config
{
    public sealed class ModConfig
    {
        public ConfigEntry<bool> Enabled { get; }
        public ConfigEntry<bool> ShowDeltasVsInstalled { get; }
        public ConfigEntry<string> EnabledStatsCsv { get; }
        public ConfigEntry<WeaponStatId> SortStat { get; }
        public ConfigEntry<SortDirection> SortDirection { get; }
        public ConfigEntry<float> OverlayFontSize { get; }
        public ConfigEntry<Color> OverlayColor { get; }
        public ConfigEntry<int> OverlayDecimalPlaces { get; }

        public ModConfig(ConfigFile config)
        {
            Enabled = config.Bind(
                "General",
                "Enabled",
                true,
                "Master toggle for attachment overlays and toolbar controls.");

            ShowDeltasVsInstalled = config.Bind(
                "Display",
                "ShowDeltasVsInstalled",
                true,
                "If true, dropdown picker tiles show delta vs the installed part. Equipped slot tiles always show absolute contribution.");

            EnabledStatsCsv = config.Bind(
                "Display",
                "EnabledStats",
                "Ergonomics,VerticalRecoil,HorizontalRecoil",
                "Comma-separated WeaponStatId values shown on each attachment tile.");

            SortStat = config.Bind(
                "Sorting",
                "SortStat",
                WeaponStatId.Ergonomics,
                "Stat used when sorting the attachment dropdown list.");

            SortDirection = config.Bind(
                "Sorting",
                "SortDirection",
                Models.SortDirection.None,
                "None disables sorting; Ascending/Descending reorder the list.");

            OverlayFontSize = config.Bind(
                "Display",
                "OverlayFontSize",
                12f,
                new ConfigDescription("Font size for tile overlays.", new AcceptableValueRange<float>(8f, 24f)));

            OverlayColor = config.Bind(
                "Display",
                "OverlayColor",
                Color.white,
                "Color for attachment tile overlay text.");

            OverlayDecimalPlaces = config.Bind(
                "Display",
                "OverlayDecimalPlaces",
                1,
                new ConfigDescription(
                    "Decimal places for overlay float values (always shown; e.g. 1 -> 1.0, 2 -> 1.25).",
                    new AcceptableValueRange<int>(0, 4)));
        }

        public HashSet<WeaponStatId> GetEnabledStats()
        {
            var result = new HashSet<WeaponStatId>();
            var raw = EnabledStatsCsv.Value ?? string.Empty;
            foreach (var part in raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Enum.TryParse(part.Trim(), ignoreCase: true, out WeaponStatId id)
                    && WeaponStatMapper.SupportsToolbar(id))
                {
                    result.Add(id);
                }
            }

            return result;
        }

        public void SetEnabledStats(IEnumerable<WeaponStatId> stats)
        {
            EnabledStatsCsv.Value = string.Join(",", stats);
        }
    }
}
