using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.WeaponModding;
using SPT.Reflection.Patching;

namespace WeaponConfigStats.Patches
{
    /// <summary>
    /// Tracks the open attachment dropdown so overlays/sorting know the active slot context.
    /// </summary>
    public sealed class DropDownMenuShowPatch : ModulePatch
    {
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
            Plugin.Log?.LogDebug("DropDownMenu.Show — attachment list opened.");
            // TODO: after CreateDropdownModdingItemView fills the list, optionally reorder
            // via AttachmentListSorter using reflection on _itemsCollection / UI children.
        }

        public static void Clear()
        {
            ActiveMenu = null;
            ActiveSlotView = null;
        }
    }
}
