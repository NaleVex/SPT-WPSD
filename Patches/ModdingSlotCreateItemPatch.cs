using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using EFT.UI.WeaponModding;
using SPT.Reflection.Patching;
using UnityEngine;
using WeaponConfigStats.UI;

namespace WeaponConfigStats.Patches
{
    /// <summary>
    /// Slot icon for the currently installed / selected part (not the dropdown list).
    /// </summary>
    public sealed class ModdingSlotCreateItemPatch : ModulePatch
    {
        private static readonly FieldInfo SlotField = typeof(ModdingScreenSlotView).GetField(
            "_slot",
            BindingFlags.Instance | BindingFlags.NonPublic);

        protected override MethodBase GetTargetMethod()
        {
            return typeof(ModdingScreenSlotView).GetMethod(
                nameof(ModdingScreenSlotView.CreateModdingItemView),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void Postfix(ModdingScreenSlotView __instance, Item item, Transform container)
        {
            if (Plugin.Settings == null || !Plugin.Settings.Enabled.Value || item == null || container == null)
            {
                return;
            }

            var view = container.GetComponentInChildren<ModdingSelectableItemView>(true);
            if (view == null)
            {
                // Newly created child is usually last.
                for (var i = container.childCount - 1; i >= 0; i--)
                {
                    view = container.GetChild(i).GetComponent<ModdingSelectableItemView>();
                    if (view != null)
                    {
                        break;
                    }
                }
            }

            if (view == null)
            {
                return;
            }

            var slot = SlotField?.GetValue(__instance) as Slot;
            var installed = slot?.ContainedItem;
            // Equipped slot tile: always absolute stats (delta vs self would be 0).
            AttachmentStatOverlay.Attach(view, item, installed, allowDeltas: false);
        }
    }
}
