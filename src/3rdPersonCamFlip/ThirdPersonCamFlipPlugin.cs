using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ThirdPersonCamFlip
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ThirdPersonCamFlipPlugin : BasePlugin
    {
        public const string PluginGuid    = "com.veryscary.a4.3rdpersoncamflip";
        public const string PluginName    = "3rdPersonCamFlip";
        public const string PluginVersion = "0.0.2-alpha.1";
        public const string ReleaseName   = "alpha 0.02";

        internal static ThirdPersonCamFlipPlugin Instance;
        internal static ConfigFile              CustomConfig;
        internal static ConfigEntry<KeyCode>    CamFlip;
        internal static ConfigEntry<int>        Mode;
        internal static ManualLogSource         LogSource;
        internal static bool                    VerboseLogging;
        internal static Harmony                 HarmonyInstance;

        public override void Load()
        {
            Instance  = this;
            LogSource = Log;

            // Use a concise config filename instead of the default GUID.
            string cfgPath = Path.Combine(Paths.ConfigPath, "3rdPersonCamFlip.cfg");
            CustomConfig = new ConfigFile(cfgPath, true);

            var verbose = CustomConfig.Bind("Logging", "Verbose", false, "Enable extra debug logs for troubleshooting.");
            VerboseLogging = verbose.Value;

            CamFlip = CustomConfig.Bind("Input", "CamFlip", KeyCode.C, "Key used to flip the third-person camera shoulder.");
            Mode = CustomConfig.Bind("Input", "Mode", 1, "1 = separate CamFlip bind; 2 = add swapped shoulder as a third camera toggle state.");

            // Register IL2CPP MonoBehaviour
            ClassInjector.RegisterTypeInIl2Cpp<CameraShoulderController>();
            // Attach controller to the BepInEx host GameObject
            AddComponent<CameraShoulderController>();

            // Harmony init + patch
            HarmonyInstance = new Harmony(PluginGuid);
            HarmonyInstance.PatchAll();

            ConsoleCommandListener.Start();

            Log.LogInfo($"[3PCF] Loaded {ReleaseName} (IL2CPP type registered, Harmony patched, Mode={Mode.Value}, CamFlip={CamFlip.Value})");
        }
    }
}
