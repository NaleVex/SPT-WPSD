using System.Collections.Generic;
using EFT.InventoryLogic;
using WPSD.Models;

namespace WPSD.Services
{
    /// <summary>
    /// Reads attachment contributions from Mod properties and Item.Attributes.
    /// When asDelta is true and an installed part is provided, returns candidate - installed.
    /// </summary>
    public static class AttachmentStatResolver
    {
        public static IReadOnlyDictionary<WeaponStatId, float> Resolve(
            Item candidate,
            Item installedInSlot,
            bool asDelta)
        {
            var candidateStats = ReadStats(candidate);
            if (!asDelta || installedInSlot == null)
            {
                return candidateStats;
            }

            var installedStats = ReadStats(installedInSlot);
            var deltas = new Dictionary<WeaponStatId, float>();
            foreach (var pair in candidateStats)
            {
                installedStats.TryGetValue(pair.Key, out var baseline);
                deltas[pair.Key] = pair.Value - baseline;
            }

            foreach (var pair in installedStats)
            {
                if (!deltas.ContainsKey(pair.Key))
                {
                    deltas[pair.Key] = -pair.Value;
                }
            }

            return deltas;
        }

        public static Dictionary<WeaponStatId, float> ReadStats(Item item)
        {
            var result = new Dictionary<WeaponStatId, float>();
            if (item == null)
            {
                return result;
            }

            if (item is Mod mod)
            {
                Set(result, WeaponStatId.Ergonomics, mod.Ergonomics);
                Set(result, WeaponStatId.Accuracy, mod.Accuracy);
                // Mods expose a single Recoil % that applies to both axes in EFT.
                Set(result, WeaponStatId.VerticalRecoil, mod.Recoil);
                Set(result, WeaponStatId.HorizontalRecoil, mod.Recoil);
                Set(result, WeaponStatId.SightingRange, mod.SightingRange);
                Set(result, WeaponStatId.MuzzleVelocity, mod.Velocity);
                Set(result, WeaponStatId.Weight, mod.Weight);
            }
            else
            {
                Set(result, WeaponStatId.Weight, item.Weight);
            }

            if (item.Attributes != null)
            {
                foreach (var attribute in item.Attributes)
                {
                    if (attribute?.Id == null || attribute.Base == null)
                    {
                        continue;
                    }

                    if (!WeaponStatMapper.TryMap(attribute.Id, out var statId))
                    {
                        continue;
                    }

                    // Prefer direct Mod properties when already set; attributes fill gaps / non-Mod items.
                    if (!result.ContainsKey(statId))
                    {
                        Set(result, statId, attribute.Base.Invoke());
                    }
                }
            }

            return result;
        }

        public static bool IsPercentStat(WeaponStatId id)
        {
            switch (id)
            {
                case WeaponStatId.Accuracy:
                case WeaponStatId.VerticalRecoil:
                case WeaponStatId.HorizontalRecoil:
                case WeaponStatId.MuzzleVelocity:
                    return true;
                default:
                    return false;
            }
        }

        public static string FormatOverlayLine(WeaponStatId id, float value, bool asDelta)
        {
            var label = ShortLabel(id);
            var number = FormatNumber(id, value);
            if (IsPercentStat(id))
            {
                number += "%";
            }

            if (asDelta)
            {
                var sign = value > 0f ? "+" : string.Empty;
                return $"{label} {sign}{number}";
            }

            return $"{label} {number}";
        }

        public static string ShortLabel(WeaponStatId id)
        {
            switch (id)
            {
                case WeaponStatId.Ergonomics: return "Ergo";
                case WeaponStatId.VerticalRecoil: return "VRec";
                case WeaponStatId.HorizontalRecoil: return "HRec";
                case WeaponStatId.Weight: return "Kg";
                case WeaponStatId.Accuracy: return "Acc";
                case WeaponStatId.SightingRange: return "Sight";
                case WeaponStatId.MuzzleVelocity: return "Vel";
                case WeaponStatId.RateOfFire: return "RoF";
                case WeaponStatId.EffectiveDistance: return "Dist";
                case WeaponStatId.Durability: return "Dur";
                default: return id.ToString();
            }
        }

        private static string FormatNumber(WeaponStatId id, float value)
        {
            // Integer-ish stats stay whole; float stats use OverlayDecimalPlaces (default 1 -> 1.0).
            if (IsIntegerStat(id))
            {
                return UnityEngine.Mathf.RoundToInt(value).ToString();
            }

            var places = 1;
            if (Plugin.Settings?.OverlayDecimalPlaces != null)
            {
                places = Plugin.Settings.OverlayDecimalPlaces.Value;
            }

            if (places < 0)
            {
                places = 0;
            }
            else if (places > 4)
            {
                places = 4;
            }

            if (places == 0)
            {
                return UnityEngine.Mathf.RoundToInt(value).ToString();
            }

            return value.ToString("0." + new string('0', places));
        }

        private static bool IsIntegerStat(WeaponStatId id)
        {
            switch (id)
            {
                case WeaponStatId.SightingRange:
                case WeaponStatId.RateOfFire:
                case WeaponStatId.EffectiveDistance:
                case WeaponStatId.Durability:
                    return true;
                default:
                    return false;
            }
        }

        private static void Set(IDictionary<WeaponStatId, float> map, WeaponStatId id, float value)
        {
            map[id] = value;
        }
    }
}
