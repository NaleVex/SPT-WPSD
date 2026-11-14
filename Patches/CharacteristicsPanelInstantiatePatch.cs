using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;
using WPSD.UI;

namespace WPSD.Patches
{
    /// <summary>
    /// Compact rows are rebuilt here after slot compare / refresh — schedule toolbar rebuild only
    /// (do not mutate hierarchy synchronously inside InstantateItemFields).
    /// </summary>
    public sealed class CharacteristicsPanelInstantiatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CharacteristicsPanel).GetMethod(
                nameof(CharacteristicsPanel.InstantiateItemFields),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
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
