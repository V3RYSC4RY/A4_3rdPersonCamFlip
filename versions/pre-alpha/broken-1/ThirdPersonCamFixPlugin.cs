using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ThirdPersonCamFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ThirdPersonCamFixPlugin : BasePlugin
    {
        public const string PluginGuid = "com.veryscary.a4.thirdpersoncamfix";
        public const string PluginName = "Third Person Cam Fix";
        public const string PluginVersion = "0.0.1";

        internal static ThirdPersonCamFixPlugin Instance;

        // This is what CameraShoulderController is trying to use
        internal static ManualLogSource LogSource
            => Instance != null
                ? Instance.Log
                : BepInEx.Logging.Logger.CreateLogSource("ThirdPersonCamFix_Fallback");

        public override void Load()
        {
            Instance = this;

            Log.LogInfo("[TPCF] Plugin loaded. Registering CameraShoulderController and ensuring host GameObject exists...");

            // Register IL2CPP MonoBehaviour
            ClassInjector.RegisterTypeInIl2Cpp<CameraShoulderController>();
            Log.LogInfo("[TPCF] Registered CameraShoulderController IL2CPP type.");

            // Try to use existing BepInEx host
            GameObject hostGo = GameObject.Find("BepInEx_Manager");

            if (hostGo != null)
            {
                Log.LogInfo("[TPCF] Found existing BepInEx_Manager GameObject; attaching CameraShoulderController.");
            }
            else
            {
                hostGo = new GameObject("BepInEx_Manager");
                Object.DontDestroyOnLoad(hostGo);
                Log.LogWarning("[TPCF] BepInEx_Manager not found; created our own persistent host GameObject.");
            }

            var existing = hostGo.GetComponent<CameraShoulderController>();
            if (existing != null)
            {
                Log.LogInfo("[TPCF] CameraShoulderController already present on host: " + existing);
                return;
            }

            var controller = hostGo.AddComponent<CameraShoulderController>();
            Log.LogInfo("[TPCF] Added CameraShoulderController component: " + controller);
        }
    }
}

