using System.Reflection;
using EFT.UI.WeaponModding;
using SPT.Reflection.Patching;

namespace WPSD.Patches
{
    public sealed class DropDownMenuClosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DropDownMenu).GetMethod(
                nameof(DropDownMenu.Close),
                BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Postfix()
        {
            DropDownMenuShowPatch.Clear();
        }
    }
}
