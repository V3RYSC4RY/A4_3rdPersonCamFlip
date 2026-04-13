using System;
using BepInEx.Logging;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

// Uses the game's CameraManager static API:
// - CameraManager.IsFirstPerson        (bool)
// - CameraManager.trackedOffsetStanding(float)
// - CameraManager.targetTrackedOffset  (float)
// - CameraManager.targetTrackedOffsetSpeed (float)
// - CameraManager.UpdateTrackedObjectOffset()

namespace ThirdPersonCamFix
{
    internal static class ShoulderState
    {
        public static bool Enabled          = true;
        public static bool UseRightShoulder = true;

        public static bool LastIsFirstPerson = true;

        public static bool LoggedError      = false;
    }

    public class CameraShoulderController : MonoBehaviour
    {
        // IL2CPP ctor boilerplate
        public CameraShoulderController(IntPtr ptr) : base(ptr) { }

        public CameraShoulderController()
            : base(ClassInjector.DerivedConstructorPointer<CameraShoulderController>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        private ManualLogSource Log => ThirdPersonCamFixPlugin.LogSource;

        // Tuning
        private const KeyCode ToggleKey        = KeyCode.C;
        private const float  FallbackOffset    = 0.6f;
        private const float  FallbackSmooth    = 10f;
        private const float  MaxLerpFactorClamp = 1f;

        private void Awake()
        {
            Log?.LogMessage("[TPCF] (CTRL) Awake – CameraManager-based shoulder controller initialized.");
        }

        private void OnEnable()
        {
            Log?.LogMessage("[TPCF] (CTRL) OnEnable; enabled=" + enabled + ", activeInHierarchy=" + gameObject.activeInHierarchy);
        }

        private void OnDisable()
        {
            Log?.LogMessage("[TPCF] (CTRL) OnDisable; enabled=" + enabled + ", activeInHierarchy=" + gameObject.activeInHierarchy);
        }

        private void OnDestroy()
        {
            Log?.LogMessage("[TPCF] (CTRL) OnDestroy on GameObject: " + gameObject.name);
        }

        private void Update()
        {
            if (!ShoulderState.Enabled)
                return;

            // 1) Read first/third person state from CameraManager
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
                Log?.LogMessage("[TPCF] (CTRL) View mode changed: " + (isFirstPerson ? "FIRST PERSON" : "THIRD PERSON"));
            }

            // Only apply shoulder logic when in third person
            if (isFirstPerson)
                return;

            // 2) Toggle shoulder side on C key
            if (Input.GetKeyDown(ToggleKey))
            {
                ShoulderState.UseRightShoulder = !ShoulderState.UseRightShoulder;
                Log?.LogMessage("[TPCF] (CTRL) Shoulder toggled: " +
                    (ShoulderState.UseRightShoulder ? "RIGHT" : "LEFT"));
            }

            // 3) Compute base offset from game's own standing offset
            float baseOffset;
            try
            {
                baseOffset = Mathf.Abs(CameraManager.trackedOffsetStanding);
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] Failed to read CameraManager.trackedOffsetStanding: " + ex);
                }
                baseOffset = FallbackOffset;
            }

            if (baseOffset < 0.01f)
                baseOffset = FallbackOffset;

            float desiredOffset = ShoulderState.UseRightShoulder ? baseOffset : -baseOffset;

            // 4) Smooth blend using game's own speed value
            float currentOffset;
            float speed;
            try
            {
                currentOffset = CameraManager.targetTrackedOffset;
                speed         = CameraManager.targetTrackedOffsetSpeed;
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] Failed to read CameraManager target offset fields: " + ex);
                }
                return;
            }

            if (speed <= 0f)
                speed = FallbackSmooth;

            float lerpFactor = Time.deltaTime * speed;
            if (lerpFactor > MaxLerpFactorClamp)
                lerpFactor = MaxLerpFactorClamp;

            float newOffset = Mathf.Lerp(currentOffset, desiredOffset, lerpFactor);

            try
            {
                // 5) Feed the offset back into the game camera system.
                // This should go through the same pipeline that already does camera collision / wall clamping.
                CameraManager.targetTrackedOffset = newOffset;
                CameraManager.UpdateTrackedObjectOffset();
            }
            catch (Exception ex)
            {
                if (!ShoulderState.LoggedError)
                {
                    ShoulderState.LoggedError = true;
                    Log?.LogError("[TPCF] Error applying shoulder offset via CameraManager: " + ex);
                }
            }
        }
    }
}
