using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ThirdPersonCamFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ThirdPersonCamFixPlugin : BasePlugin
    {
        public const string PluginGuid    = "com.veryscary.a4.thirdpersoncamfix";
        public const string PluginName    = "Third Person Cam Fix";
        public const string PluginVersion = "0.0.4";

        internal static ThirdPersonCamFixPlugin Instance;
        internal static ConfigFile              CustomConfig;
        internal static ConfigEntry<KeyCode>    SwapKey;
        internal static ManualLogSource         LogSource;
        internal static bool                    VerboseLogging;
        internal static Harmony                 HarmonyInstance;

        public override void Load()
        {
            Instance  = this;
            LogSource = Log;

            // Use a concise config filename instead of the default GUID.
            string cfgPath = Path.Combine(Paths.ConfigPath, "TPCF.cfg");
            CustomConfig = new ConfigFile(cfgPath, true);

            var verbose = CustomConfig.Bind("Logging", "Verbose", false, "Enable extra debug logs for troubleshooting.");
            VerboseLogging = verbose.Value;

            SwapKey = CustomConfig.Bind("Input", "SwapKey", KeyCode.C, "Key used to swap the third-person camera shoulder.");

            // Register IL2CPP MonoBehaviour
            ClassInjector.RegisterTypeInIl2Cpp<CameraShoulderController>();
            // Attach controller to the BepInEx host GameObject
            AddComponent<CameraShoulderController>();

            // Harmony init + patch
            HarmonyInstance = new Harmony(PluginGuid);
            HarmonyInstance.PatchAll();

            Log.LogInfo($"[TPCF] Loaded 0.0.4 (IL2CPP type registered, Harmony patched, SwapKey={SwapKey.Value})");
        }
    }
}
