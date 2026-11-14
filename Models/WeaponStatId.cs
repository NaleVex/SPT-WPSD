namespace WPSD.Models
{
    /// <summary>
    /// Stats we can toggle/sort/overlay. Maps loosely to EItemAttributeId / CharacteristicsPanel rows.
    /// </summary>
    public enum WeaponStatId
    {
        Durability,
        Weight,
        Ergonomics,
        Accuracy,
        SightingRange,
        VerticalRecoil,
        HorizontalRecoil,
        MuzzleVelocity,
        RateOfFire,
        EffectiveDistance
    }

    public enum SortDirection
    {
        None = 0,
        Ascending = 1,
        Descending = 2
    }
}
