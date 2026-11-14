using System;
using EFT.InventoryLogic;
using WeaponConfigStats.Models;

namespace WeaponConfigStats.Services
{
    public static class WeaponStatMapper
    {
        public static bool TryMap(Enum id, out WeaponStatId statId)
        {
            statId = default;
            if (!(id is EItemAttributeId attr))
            {
                return false;
            }

            switch (attr)
            {
                case EItemAttributeId.Weight:
                    statId = WeaponStatId.Weight;
                    return true;
                case EItemAttributeId.Ergonomics:
                    statId = WeaponStatId.Ergonomics;
                    return true;
                case EItemAttributeId.Accuracy:
                case EItemAttributeId.CenterOfImpact:
                    statId = WeaponStatId.Accuracy;
                    return true;
                case EItemAttributeId.SightingRange:
                    statId = WeaponStatId.SightingRange;
                    return true;
                case EItemAttributeId.RecoilUp:
                case EItemAttributeId.Recoil:
                    statId = WeaponStatId.VerticalRecoil;
                    return true;
                case EItemAttributeId.RecoilBack:
                    statId = WeaponStatId.HorizontalRecoil;
                    return true;
                case EItemAttributeId.Velocity:
                case EItemAttributeId.BulletSpeed:
                    statId = WeaponStatId.MuzzleVelocity;
                    return true;
                // Durability / fire rate / effective distance: no toolbar toggles or overlays.
                default:
                    return false;
            }
        }

        public static bool SupportsToolbar(WeaponStatId id)
        {
            switch (id)
            {
                case WeaponStatId.Durability:
                case WeaponStatId.RateOfFire:
                case WeaponStatId.EffectiveDistance:
                    return false;
                default:
                    return true;
            }
        }
    }
}
