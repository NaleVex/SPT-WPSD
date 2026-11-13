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
                case EItemAttributeId.Durability:
                case EItemAttributeId.MaxDurability:
                    statId = WeaponStatId.Durability;
                    return true;
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
                case EItemAttributeId.FireRate:
                case EItemAttributeId.SingleFireRate:
                    statId = WeaponStatId.RateOfFire;
                    return true;
                case EItemAttributeId.EffectiveDist:
                case EItemAttributeId.EffectiveDistance:
                    statId = WeaponStatId.EffectiveDistance;
                    return true;
                default:
                    return false;
            }
        }
    }
}
