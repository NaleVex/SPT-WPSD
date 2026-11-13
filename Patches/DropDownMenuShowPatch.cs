using System.Collections;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.WeaponModding;
using SPT.Reflection.Patching;
using WeaponConfigStats.Services;

namespace WeaponConfigStats.Patches
{
    /// <summary>
    /// Tracks the open attachment dropdown and applies active sort after the list is built.
    /// </summary>
    public sealed class DropDownMenuShowPatch : ModulePatch
    {
        private static readonly FieldInfo SlotField = typeof(ModdingScreenSlotView).GetField(
            "_slot",
            BindingFlags.Instance | BindingFlags.NonPublic);

        public static DropDownMenu ActiveMenu { get; private set; }
        public static ModdingScreenSlotView ActiveSlotView { get; private set; }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(DropDownMenu).GetMethod(
                nameof(DropDownMenu.Show),
                BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Postfix(DropDownMenu __instance, ModdingScreenSlotView slotView)
        {
            ActiveMenu = __instance;
            ActiveSlotView = slotView;
            ApplyCurrentSort();
            if (Plugin.Instance != null)
            {
                Plugin.Instance.StartCoroutine(ApplyNextFrame());
            }
        }

        public static void Clear()
        {
            ActiveMenu = null;
            ActiveSlotView = null;
            AttachmentListSorter.ClearSnapshot();
        }

        public static Item ResolveInstalledItem(ModdingScreenSlotView slotView)
        {
            if (slotView == null || SlotField == null)
            {
                return null;
            }

            var slot = SlotField.GetValue(slotView) as Slot;
            return slot?.ContainedItem;
        }

        public static void ApplyCurrentSort()
        {
            if (Plugin.Settings == null || !Plugin.Settings.Enabled.Value)
            {
                return;
            }

            var menu = ActiveMenu;
            if (menu == null)
            {
                return;
            }

            var selection = Plugin.Selection;
            if (selection == null)
            {
                return;
            }

            var installed = ResolveInstalledItem(ActiveSlotView);
            AttachmentListSorter.ApplyToOpenMenu(
                menu,
                installed,
                selection.SortStat,
                selection.SortDirection,
                selection.ShowDeltas);
        }

        private static IEnumerator ApplyNextFrame()
        {
            yield return null;
            if (ActiveMenu != null)
            {
                ApplyCurrentSort();
            }
        }
    }
}
