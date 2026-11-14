using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;
using WPSD.UI;

namespace WPSD.Patches
{
    public sealed class CharacteristicsPanelShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CharacteristicsPanel).GetMethod(
                nameof(CharacteristicsPanel.Show),
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(Item), typeof(Item) },
                null);
        }

        [PatchPostfix]
        private static void Postfix(CharacteristicsPanel __instance)
        {
            if (Plugin.Settings == null || !Plugin.Settings.Enabled.Value)
            {
                return;
            }

            StatToolbarController.EnsureOn(__instance);
        }
    }
}
