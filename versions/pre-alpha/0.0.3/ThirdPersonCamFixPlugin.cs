using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace ThirdPersonCamFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ThirdPersonCamFixPlugin : BasePlugin
    {
        public const string PluginGuid    = "com.veryscary.a4.thirdpersoncamfix";
        public const string PluginName    = "Third Person Cam Fix";
        public const string PluginVersion = "0.0.3";

        internal static ThirdPersonCamFixPlugin Instance;
        internal static ManualLogSource         LogSource;
        internal static Harmony                 HarmonyInstance;

        public override void Load()
        {
            Instance  = this;
            LogSource = Log;

            Log.LogInfo("[TPCF] Plugin loaded. Registering CameraShoulderController and applying Harmony patches...");

            // Register IL2CPP MonoBehaviour
            ClassInjector.RegisterTypeInIl2Cpp<CameraShoulderController>();
            Log.LogInfo("[TPCF] Registered CameraShoulderController IL2CPP type.");

            // Attach controller to the BepInEx host GameObject
            var controller = AddComponent<CameraShoulderController>();
            Log.LogInfo("[TPCF] Added CameraShoulderController via AddComponent: " + controller);

            // Harmony init + patch
            HarmonyInstance = new Harmony(PluginGuid);
            HarmonyInstance.PatchAll();
            Log.LogInfo("[TPCF] Harmony patches applied.");
        }
    }
}
