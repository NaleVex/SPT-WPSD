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

            new CharacteristicsPanelShowPatch().Enable();
            new CharacteristicsPanelInstantiatePatch().Enable();
            new CharacteristicsPanelSetStatePatch().Enable();
            new DropDownMenuShowPatch().Enable();
            new DropDownMenuCreateItemPatch().Enable();
            new DropDownMenuClosePatch().Enable();
            new ModdingSlotCreateItemPatch().Enable();

            Log.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded.");
        }
    }

    public static class PluginInfo
    {
        public const string Guid = "com.weaponconfigstats.client";
        public const string Name = "WeaponConfigStats";
        public const string Version = "0.1.6";
    }
}
