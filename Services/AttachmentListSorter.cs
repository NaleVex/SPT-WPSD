using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using EFT.UI.WeaponModding;
using UnityEngine;
using UnityEngine.UI;
using WeaponConfigStats.Models;

namespace WeaponConfigStats.Services
{
    /// <summary>
    /// Reorders attachment dropdown tiles by a selected weapon stat.
    /// </summary>
    public static class AttachmentListSorter
    {
        // _itemsContainer is a public Unity-serialized field; NonPublic-only GetField returns null.
        private static readonly FieldInfo ItemsContainerField = typeof(DropDownMenu).GetField(
            "_itemsContainer",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo ItemsCollectionField = typeof(DropDownMenu).GetField(
            "_itemsCollection",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly MethodInfo UpdatePositionMethod = typeof(DropDownMenu).GetMethod(
            nameof(DropDownMenu.UpdatePosition),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static DropDownMenu _snapshotMenu;
        private static List<string> _naturalOrderIds;

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

            var indexed = list
                .Select((item, index) => (item, index))
                .ToList();

            var sign = direction == SortDirection.Ascending ? 1 : -1;
            indexed.Sort((a, b) =>
            {
                var cmp = CompareStat(a.item, b.item, installedInSlot, sortStat, asDelta) * sign;
                return cmp != 0 ? cmp : a.index.CompareTo(b.index);
            });

            return indexed.Select(x => x.item).ToList();
        }

        public static void ClearSnapshot()
        {
            _snapshotMenu = null;
            _naturalOrderIds = null;
        }

        public static void ApplyToOpenMenu(
            DropDownMenu menu,
            Item installedInSlot,
            WeaponStatId sortStat,
            SortDirection direction,
            bool asDelta)
        {
            if (menu == null || ItemsContainerField == null || ItemsCollectionField == null)
            {
                return;
            }

            var container = ItemsContainerField.GetValue(menu) as RectTransform;
            var collection = ItemsCollectionField.GetValue(menu) as List<Item>;
            if (container == null)
            {
                return;
            }

            var entries = CollectEntries(container);
            if (entries.Count == 0)
            {
                entries = CollectEntriesDeep(container);
            }

            if (entries.Count == 0)
            {
                return;
            }

            EnsureNaturalOrderSnapshot(menu, entries);

            if (direction == SortDirection.None)
            {
                RestoreNaturalOrder(entries);
            }
            else
            {
                var sign = direction == SortDirection.Ascending ? 1 : -1;
                entries.Sort((a, b) =>
                {
                    var cmp = CompareStat(a.Item, b.Item, installedInSlot, sortStat, asDelta) * sign;
                    return cmp != 0 ? cmp : a.NaturalIndex.CompareTo(b.NaturalIndex);
                });
            }

            for (var i = 0; i < entries.Count; i++)
            {
                entries[i].Root.SetSiblingIndex(i);
            }

            if (collection != null)
            {
                collection.Clear();
                collection.AddRange(entries.Select(e => e.Item));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            UpdatePositionMethod?.Invoke(menu, null);
        }

        private static int CompareStat(
            Item a,
            Item b,
            Item installedInSlot,
            WeaponStatId sortStat,
            bool asDelta)
        {
            var av = AttachmentStatResolver.Resolve(a, installedInSlot, asDelta);
            var bv = AttachmentStatResolver.Resolve(b, installedInSlot, asDelta);
            av.TryGetValue(sortStat, out var aVal);
            bv.TryGetValue(sortStat, out var bVal);
            return aVal.CompareTo(bVal);
        }

        private static void EnsureNaturalOrderSnapshot(DropDownMenu menu, List<Entry> entries)
        {
            if (ReferenceEquals(_snapshotMenu, menu) && _naturalOrderIds != null)
            {
                foreach (var entry in entries)
                {
                    var id = ItemKey(entry.Item);
                    var idx = _naturalOrderIds.IndexOf(id);
                    entry.NaturalIndex = idx >= 0 ? idx : entry.NaturalIndex;
                }

                return;
            }

            _snapshotMenu = menu;
            _naturalOrderIds = entries.Select(e => ItemKey(e.Item)).ToList();
            for (var i = 0; i < entries.Count; i++)
            {
                entries[i].NaturalIndex = i;
            }
        }

        private static void RestoreNaturalOrder(List<Entry> entries)
        {
            if (_naturalOrderIds == null)
            {
                entries.Sort((a, b) => a.NaturalIndex.CompareTo(b.NaturalIndex));
                return;
            }

            entries.Sort((a, b) =>
            {
                var ai = _naturalOrderIds.IndexOf(ItemKey(a.Item));
                var bi = _naturalOrderIds.IndexOf(ItemKey(b.Item));
                if (ai < 0)
                {
                    ai = int.MaxValue;
                }

                if (bi < 0)
                {
                    bi = int.MaxValue;
                }

                var cmp = ai.CompareTo(bi);
                return cmp != 0 ? cmp : a.NaturalIndex.CompareTo(b.NaturalIndex);
            });
        }

        private static List<Entry> CollectEntries(Transform container)
        {
            var entries = new List<Entry>();
            if (container == null)
            {
                return entries;
            }

            for (var i = 0; i < container.childCount; i++)
            {
                var child = container.GetChild(i);
                var view = child.GetComponent<ModdingSelectableItemView>()
                    ?? child.GetComponentInChildren<ModdingSelectableItemView>(true);
                if (view?.Item == null)
                {
                    continue;
                }

                entries.Add(new Entry
                {
                    Root = child,
                    View = view,
                    Item = view.Item,
                    NaturalIndex = i
                });
            }

            return entries;
        }

        private static List<Entry> CollectEntriesDeep(Transform container)
        {
            var entries = new List<Entry>();
            if (container == null)
            {
                return entries;
            }

            var views = container.GetComponentsInChildren<ModdingSelectableItemView>(true);
            var seen = new HashSet<Transform>();
            for (var i = 0; i < views.Length; i++)
            {
                var view = views[i];
                if (view?.Item == null)
                {
                    continue;
                }

                var root = view.transform;
                while (root.parent != null && root.parent != container)
                {
                    root = root.parent;
                }

                if (root.parent != container || !seen.Add(root))
                {
                    continue;
                }

                entries.Add(new Entry
                {
                    Root = root,
                    View = view,
                    Item = view.Item,
                    NaturalIndex = root.GetSiblingIndex()
                });
            }

            entries.Sort((a, b) => a.NaturalIndex.CompareTo(b.NaturalIndex));
            return entries;
        }

        private static string ItemKey(Item item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            return item.Id ?? item.TemplateId.ToString();
        }

        private sealed class Entry
        {
            public Transform Root;
            public ModdingSelectableItemView View;
            public Item Item;
            public int NaturalIndex;
        }
    }
}
