using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;
using WeaponConfigStats.UI;

namespace WeaponConfigStats.Patches
{
    /// <summary>
    /// Compact rows often exist while _characteristics is collapsed (inactive hierarchy).
    /// Rebuild toolbar when the info panel expands so controls appear with the list.
    /// </summary>
    public sealed class CharacteristicsPanelSetStatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CharacteristicsPanel).GetMethod(
                nameof(CharacteristicsPanel.SetState),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(bool) },
                null);
        }

        [PatchPostfix]
        private static void Postfix(CharacteristicsPanel __instance, bool expanded)
        {
            if (!expanded)
            {
                return;
            }

            if (Plugin.Settings == null || !Plugin.Settings.Enabled.Value)
            {
                return;
            }

            StatToolbarController.EnsureOn(__instance);
        }
    }
}
