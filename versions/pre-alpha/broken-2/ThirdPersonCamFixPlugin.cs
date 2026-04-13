using System;
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
        public const string PluginGuid    = "com.veryscary.a4.thirdpersoncamfix";
        public const string PluginName    = "Third Person Cam Fix";
        public const string PluginVersion = "0.0.3";

        internal static ManualLogSource LogSource;

        public override void Load()
        {
            LogSource = Log;
            LogSource.LogInfo("[TPCF] v0.0.3 loaded – registering IL2CPP types and creating controller host...");

            // Register the IL2CPP MonoBehaviour type
            ClassInjector.RegisterTypeInIl2Cpp<CameraShoulderController>();
            LogSource.LogInfo("[TPCF] Registered CameraShoulderController IL2CPP type.");

            // Create a persistent host GameObject for our controller
            var hostGo = new GameObject("ThirdPersonCamFix_ControllerHost");
            UnityEngine.Object.DontDestroyOnLoad(hostGo);
            hostGo.hideFlags = HideFlags.HideAndDontSave;

            var controller = hostGo.AddComponent<CameraShoulderController>();
            LogSource.LogInfo("[TPCF] Attached CameraShoulderController to host: " + controller);
        }
    }
}
