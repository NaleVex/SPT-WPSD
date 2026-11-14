using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using EFT.UI.WeaponModding;
using SPT.Reflection.Patching;
using UnityEngine;
using WPSD.UI;

namespace WPSD.Patches
{
    /// <summary>
    /// After each attachment tile is created in the dropdown, attach the orange-area overlay.
    /// </summary>
    public sealed class DropDownMenuCreateItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DropDownMenu).GetMethod(
                nameof(DropDownMenu.CreateDropdownModdingItemView),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void Postfix(DropDownMenu __instance, Item item, RectTransform container)
        {
            if (Plugin.Settings == null || !Plugin.Settings.Enabled.Value || item == null || container == null)
            {
                return;
            }

            var view = FindLatestView(container);
            if (view == null)
            {
                Plugin.Log?.LogDebug($"No ModdingSelectableItemView found for {item.TemplateId}");
                return;
            }

            var installed = DropDownMenuShowPatch.ResolveInstalledItem(DropDownMenuShowPatch.ActiveSlotView);
            // Picker list: deltas vs installed when ShowDeltasVsInstalled is on.
            AttachmentStatOverlay.Attach(view, item, installed, allowDeltas: true);
        }

        private static ModdingSelectableItemView FindLatestView(RectTransform container)
        {
            for (var i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                var view = child.GetComponent<ModdingSelectableItemView>();
                if (view != null)
                {
                    return view;
                }

                view = child.GetComponentInChildren<ModdingSelectableItemView>(true);
                if (view != null)
                {
                    return view;
                }
            }

            return null;
        }
    }
}
