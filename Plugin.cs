using BepInEx;
using BepInEx.Logging;
using WeaponConfigStats.Config;
using WeaponConfigStats.Patches;
using WeaponConfigStats.Services;

namespace WeaponConfigStats
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("com.SPT.core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }
        public static ModConfig Settings { get; private set; }
        public static StatSelectionService Selection { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Settings = new ModConfig(Config);
            Selection = new StatSelectionService(Settings);
            Selection.Changed += OnSelectionChanged;
            Settings.OverlayColor.SettingChanged += (_, __) => OnSelectionChanged();
            Settings.OverlayFontSize.SettingChanged += (_, __) => OnSelectionChanged();
            Settings.OverlayDecimalPlaces.SettingChanged += (_, __) => OnSelectionChanged();

            new CharacteristicsPanelShowPatch().Enable();
            new CharacteristicsPanelInstantiatePatch().Enable();
            new CharacteristicsPanelSetStatePatch().Enable();
            new DropDownMenuShowPatch().Enable();
            new DropDownMenuCreateItemPatch().Enable();
            new DropDownMenuClosePatch().Enable();
            new ModdingSlotCreateItemPatch().Enable();

            Log.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded.");
        }

        private void OnDestroy()
        {
            if (Selection != null)
            {
                Selection.Changed -= OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged()
        {
            DropDownMenuShowPatch.ApplyCurrentSort();
        }
    }

    public static class PluginInfo
    {
        // Forge: GUID must match the upload form; Name must be Username-ModName (letters/numbers/one dash).
        public const string Guid = "com.funapple.weaponconfigstats";
        public const string Name = "FunApple-WeaponPartsStatsDisplay";
        public const string Version = "1.0.1";
    }
}
