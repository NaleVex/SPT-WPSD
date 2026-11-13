using System.Collections.Generic;
using EFT.InventoryLogic;
using WeaponConfigStats.Models;

namespace WeaponConfigStats.Services
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
                Set(result, WeaponStatId.EffectiveDistance, mod.EffectiveDistance);
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

        public static string FormatOverlayLine(WeaponStatId id, float value, bool asDelta)
        {
            var label = ShortLabel(id);
            if (asDelta)
            {
                var sign = value > 0f ? "+" : string.Empty;
                return $"{label} {sign}{FormatNumber(id, value)}";
            }

            return $"{label} {FormatNumber(id, value)}";
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
            switch (id)
            {
                case WeaponStatId.Weight:
                    return value.ToString("0.##");
                case WeaponStatId.Ergonomics:
                case WeaponStatId.VerticalRecoil:
                case WeaponStatId.HorizontalRecoil:
                case WeaponStatId.MuzzleVelocity:
                    return value.ToString("0.#");
                case WeaponStatId.Accuracy:
                    return value.ToString("0.##");
                default:
                    return value.ToString("0.#");
            }
        }

        private static void Set(IDictionary<WeaponStatId, float> map, WeaponStatId id, float value)
        {
            map[id] = value;
        }
    }
}
