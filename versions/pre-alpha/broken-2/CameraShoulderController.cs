using System;
using BepInEx.Logging;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

// Relies on the game's CameraManager:
//
// public class CameraManager : Singleton<CameraManager>
// {
//     public static bool IsFirstPerson;
//     public float trackedOffsetStanding;
//     public float targetTrackedOffset;
//     public float targetTrackedOffsetSpeed;
//     public void UpdateTrackedObjectOffset();
//     public static CameraManager Instance { get; }
// }

namespace ThirdPersonCamFix
{
    internal static class ShoulderState
    {
        public static bool Enabled          = true;
        public static bool UseRightShoulder = true;

        public static bool LastIsFirstPerson = true;
        public static bool LoggedError       = false;
    }

    public class CameraShoulderController : MonoBehaviour
    {
        // IL2CPP boilerplate
        public CameraShoulderController(IntPtr ptr) : base(ptr) { }

        public CameraShoulderController()
            : base(ClassInjector.DerivedConstructorPointer<CameraShoulderController>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        private ManualLogSource Log => ThirdPersonCamFixPlugin.LogSource;

        // Tuning
        private const KeyCode ToggleKey          = KeyCode.C;
        private const float  FallbackOffset      = 0.6f;
        private const float  FallbackSmooth      = 10f;
        private const float  MaxLerpFactorClamp  = 1f;

        private void Awake()
        {
            Log?.LogMessage("[TPCF] (CTRL) Awake – CameraManager-based shoulder controller initialized.");
        }

        private void OnEnable()
        {
            Log?.LogMessage($"[TPCF] (CTRL) OnEnable; enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");
        }

        private void OnDisable()
        {
            Log?.LogMessage($"[TPCF] (CTRL) OnDisable; enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");
        }

        private void OnDestroy()
        {
            Log?.LogMessage("[TPCF] (CTRL) OnDestroy on GameObject: " + gameObject.name);
        }

        private void Update()
        {
            if (!ShoulderState.Enabled)
                return;

            // 0) Grab CameraManager singleton instance
            CameraManager camMgr = null;
            try
            {
                camMgr = CameraManager.Instance;
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] CameraManager.Instance threw: " + ex);
                }
                return;
            }

            if (camMgr == null)
            {
                // No manager yet (e.g. in menus)
                return;
            }

            // 1) Read first/third person flag (static)
            bool isFirstPerson;
            try
            {
                isFirstPerson = CameraManager.IsFirstPerson;
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] Failed to read CameraManager.IsFirstPerson: " + ex);
                }
                return;
            }

            if (isFirstPerson != ShoulderState.LastIsFirstPerson)
            {
                ShoulderState.LastIsFirstPerson = isFirstPerson;
                Log?.LogMessage("[TPCF] (CTRL) View mode changed: " +
                                (isFirstPerson ? "FIRST PERSON" : "THIRD PERSON"));
            }

            // Only do shoulder stuff in third person
            if (isFirstPerson)
                return;

            // 2) Toggle shoulder side on C
            if (Input.GetKeyDown(ToggleKey))
            {
                ShoulderState.UseRightShoulder = !ShoulderState.UseRightShoulder;
                Log?.LogMessage("[TPCF] (CTRL) Shoulder toggled: " +
                                (ShoulderState.UseRightShoulder ? "RIGHT" : "LEFT"));
            }

            // 3) Base offset from game's own standing offset (instance field)
            float baseOffset;
            try
            {
                baseOffset = Mathf.Abs(camMgr.trackedOffsetStanding);
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] Failed to read camMgr.trackedOffsetStanding: " + ex);
                }
                baseOffset = FallbackOffset;
            }

            if (baseOffset < 0.01f)
                baseOffset = FallbackOffset;

            float desiredOffset = ShoulderState.UseRightShoulder ? baseOffset : -baseOffset;

            // 4) Smooth via instance fields
            float currentOffset;
            float speed;
            try
            {
                currentOffset = camMgr.targetTrackedOffset;
                speed         = camMgr.targetTrackedOffsetSpeed;
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] Failed to read camMgr.targetTrackedOffset/Speed: " + ex);
                }
                return;
            }

            if (speed <= 0f)
                speed = FallbackSmooth;

            float lerpFactor = Time.deltaTime * speed;
            if (lerpFactor > MaxLerpFactorClamp)
                lerpFactor = MaxLerpFactorClamp;

            float newOffset = Mathf.Lerp(currentOffset, desiredOffset, lerpFactor);

            // 5) Feed back into CameraManager so its own camera pipeline (including wall clamping) is used
            try
            {
                camMgr.targetTrackedOffset = newOffset;
                camMgr.UpdateTrackedObjectOffset();
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] Error applying shoulder offset via CameraManager instance: " + ex);
                }
            }
        }
    }
}
