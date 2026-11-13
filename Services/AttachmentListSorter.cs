using System.Collections.Generic;
using System.Linq;
using EFT.InventoryLogic;
using WeaponConfigStats.Models;

namespace WeaponConfigStats.Services
{
    /// <summary>
    /// Stub sorter for dropdown candidates. Wire into DropDownMenu._itemsCollection when ready.
    /// </summary>
    public static class AttachmentListSorter
    {
        public static List<Item> Sort(
            IEnumerable<Item> items,
            Item installedInSlot,
            WeaponStatId sortStat,
            SortDirection direction,
            bool asDelta)
        {
            var list = items?.ToList() ?? new List<Item>();
            if (direction == SortDirection.None || list.Count <= 1)
            {
                return list;
            }

            int Sign() => direction == SortDirection.Ascending ? 1 : -1;

            list.Sort((a, b) =>
            {
                var av = AttachmentStatResolver.Resolve(a, installedInSlot, asDelta);
                var bv = AttachmentStatResolver.Resolve(b, installedInSlot, asDelta);
                av.TryGetValue(sortStat, out var aVal);
                bv.TryGetValue(sortStat, out var bVal);
                return aVal.CompareTo(bVal) * Sign();
            });

            return list;
        }
    }
}
