using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThirdPersonCamFlip
{
    /// <summary>
    /// Global shoulder state owned by the plugin.
    /// The Harmony patch reads from this when actually moving the camera.
    /// </summary>
    internal static class ShoulderState
    {
        public static bool  InThirdPerson          = false;
        public static bool  Enabled                = true;  // future toggle if you want keybind to disable
        public static bool  UseCameraManagerOffset = true;  // preferred pipeline

        // +1 = default camera side, -1 = mirrored alternate side. We lerp between them for smoothing.
        public static float CurrentShoulderSign    = 1f;
        public static float TargetShoulderSign     = 1f;

        // Used only if the live camera has no baked-in lateral offset to mirror.
        public static float FallbackShoulderOffset = 1f;
        public static float FallbackTrackedOffset  = 0.6f; // for CameraManager path when the game reports 0

        public static void UseDefaultShoulder(bool snap = false)
        {
            TargetShoulderSign = 1f;
            if (snap)
                CurrentShoulderSign = TargetShoulderSign;
        }

        public static void UseSwappedShoulder(bool snap = false)
        {
            TargetShoulderSign = -1f;
            if (snap)
                CurrentShoulderSign = TargetShoulderSign;
        }

        public static bool IsSwapped => TargetShoulderSign < 0f;

        public static void ResetRuntimeState()
        {
            InThirdPerson = false;
            CurrentShoulderSign = 1f;
            TargetShoulderSign = 1f;
            FallbackShoulderOffset = 1f;
            FallbackTrackedOffset = 0.6f;
        }
    }

    public class CameraShoulderController : MonoBehaviour
    {
        private static ManualLogSource Log => ThirdPersonCamFlipPlugin.LogSource;
        private static bool Verbose => ThirdPersonCamFlipPlugin.VerboseLogging;
        private const string Prefix = "[3PCF] (CTRL)";
        private const float WheelTiltThreshold = 0.01f;
        private const int Mode1 = 1;
        private const int Mode3A = 3;
        private const int Mode3B = 4;

        // Local state
        private bool  _lastIsFirstPerson = true;

        // true = default camera side, false = mirrored alternate side.
        private bool  _useDefaultShoulder  = true;

        // These are just *our* tuning knobs / fallbacks
        private float _baseOffset        = 1f;   // used only if the camera provides no baked-in offset
        private float _lerpSpeed         = 8f;   // how fast to blend
        private float _nextHeartbeat;
        private float _nextSceneCheck;
        private int _lastSceneHandle;

        private void Awake()
        {
            if (!Verbose)
                return;
            Log?.LogDebug($"{Prefix} Awake on GameObject: {gameObject.name}");
        }

        private void OnEnable()
        {
            if (!Verbose)
                return;
            Log?.LogDebug($"{Prefix} OnEnable; enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");
        }

        private void OnDisable()
        {
            if (!Verbose)
                return;
            Log?.LogDebug($"{Prefix} OnDisable; enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");
        }

        private void OnDestroy()
        {
            if (!Verbose)
                return;
            Log?.LogDebug($"{Prefix} OnDestroy on GameObject: {gameObject.name}");
        }

        private void Update()
        {
            float t = Time.time;
            CheckSceneChanged(t);

            // --- Heartbeat every ~1.5s so we know Update is alive (quieted) ---
            if (t >= _nextHeartbeat)
            {
                _nextHeartbeat = t + 1.5f;
                // Log?.LogMessage($"{Prefix} Update heartbeat - Time.time={t}");
            }

            if (!ShoulderState.Enabled)
                return;

            ShoulderState.FallbackShoulderOffset = Mathf.Abs(_baseOffset);

            // --- Grab CameraManager singleton safely ---
            CameraManager camMgr = null;
            try
            {
                camMgr = CameraManager.Instance;
            }
            catch
            {
                // If this blows up, we just skip this frame
            }

            if (camMgr == null)
                return;

            // --- Read current view mode (first vs third person) ---
            bool isFirst = false;
            try
            {
                isFirst = CameraManager.IsFirstPerson;
            }
            catch
            {
                // If this property explodes, bail but don't kill the mod
                return;
            }

            // Track mode changes + log once
            if (isFirst != _lastIsFirstPerson)
            {
                _lastIsFirstPerson = isFirst;
                if (Verbose)
                    Log?.LogDebug($"{Prefix} View mode changed: {(isFirst ? "FIRST PERSON" : "THIRD PERSON")}");
            }

            ShoulderState.InThirdPerson = !isFirst;
            int mode = ThirdPersonCamFlipPlugin.Mode?.Value ?? Mode1;
            SyncLocalShoulderFromState();

            if (UiInputFocus.IsTextInputActive())
            {
                SmoothShoulder();
                return;
            }

            // Mode 3b can enter third-person directly from first-person wheel tilt.
            if (isFirst)
            {
                if (mode == Mode3B && TryGetWheelTiltVisualRight(out bool useVisualRightShoulder))
                {
                    SetVisualRightShoulder(useVisualRightShoulder, true);
                    TryEnterThirdPerson();

                    if (Verbose)
                    {
                        Log?.LogDebug(
                            $"{Prefix} Mode 3b wheel tilt selected {(useVisualRightShoulder ? "RIGHT" : "LEFT")} " +
                            "and entered third person."
                        );
                    }
                }

                // Do not reset side; keep last third-person side remembered.
                // Still keep the lerp alive toward the last target so we resume smoothly.
                SmoothShoulder();
                return;
            }

            // --- Mode 1: shoulder flip (configurable; defaults to C) ---
            KeyCode camFlip = ThirdPersonCamFlipPlugin.CamFlip?.Value ?? KeyCode.C;
            if (mode == Mode1 &&
                camFlip != KeyCode.None &&
                Input.GetKeyDown(camFlip))
            {
                // Flip between default and mirrored alternate.
                SetDefaultShoulder(!_useDefaultShoulder);

                if (Verbose)
                {
                    Log?.LogDebug(
                    $"{Prefix} Shoulder toggled: {(_useDefaultShoulder ? "DEFAULT" : "ALTERNATE")} " +
                    $"TargetSign={ShoulderState.TargetShoulderSign}"
                );
                }
            }

            // --- Modes 3a/3b: mouse wheel tilt chooses explicit shoulder orientation ---
            if (mode == Mode3A || mode == Mode3B)
            {
                if (TryGetWheelTiltVisualRight(out bool useVisualRightShoulder))
                {
                    SetVisualRightShoulder(useVisualRightShoulder);
                    if (Verbose)
                    {
                        Log?.LogDebug(
                            $"{Prefix} Wheel tilt selected {(useVisualRightShoulder ? "RIGHT" : "LEFT")} " +
                            $"DefaultIsRight={CinemachineBrainPatches.IsDefaultShoulderVisuallyRight()} " +
                            $"TargetSign={ShoulderState.TargetShoulderSign}"
                        );
                    }
                }
            }

            SmoothShoulder();
        }

        private void SetDefaultShoulder(bool useDefaultShoulder, bool snap = false)
        {
            _useDefaultShoulder = useDefaultShoulder;
            ShoulderState.TargetShoulderSign = _useDefaultShoulder ? 1f : -1f;
            if (snap)
                ShoulderState.CurrentShoulderSign = ShoulderState.TargetShoulderSign;
        }

        private void SyncLocalShoulderFromState()
        {
            _useDefaultShoulder = ShoulderState.TargetShoulderSign >= 0f;
        }

        private void SetVisualRightShoulder(bool useVisualRightShoulder, bool snap = false)
        {
            bool defaultIsVisualRight = CinemachineBrainPatches.IsDefaultShoulderVisuallyRight();
            SetDefaultShoulder(useVisualRightShoulder == defaultIsVisualRight, snap);
        }

        private bool TryGetWheelTiltVisualRight(out bool useVisualRightShoulder)
        {
            float horizontalTilt = Input.mouseScrollDelta.x;
            bool invert = ThirdPersonCamFlipPlugin.InvertWheelTilt?.Value ?? false;

            if (horizontalTilt <= -WheelTiltThreshold)
            {
                useVisualRightShoulder = invert;
                LogWheelTilt(horizontalTilt, useVisualRightShoulder);
                return true;
            }

            if (horizontalTilt >= WheelTiltThreshold)
            {
                useVisualRightShoulder = !invert;
                LogWheelTilt(horizontalTilt, useVisualRightShoulder);
                return true;
            }

            useVisualRightShoulder = _useDefaultShoulder == CinemachineBrainPatches.IsDefaultShoulderVisuallyRight();
            return false;
        }

        private void LogWheelTilt(float rawTilt, bool useVisualRightShoulder)
        {
            if (!Verbose)
                return;

            bool defaultIsVisualRight = CinemachineBrainPatches.IsDefaultShoulderVisuallyRight();
            Log?.LogDebug(
                $"{Prefix} WheelTilt rawX={rawTilt:F3} invert={ThirdPersonCamFlipPlugin.InvertWheelTilt?.Value ?? false} " +
                $"requested={(useVisualRightShoulder ? "RIGHT" : "LEFT")} defaultIsRight={defaultIsVisualRight}"
            );
        }

        private void TryEnterThirdPerson()
        {
            try
            {
                CameraManager.IsFirstPerson = false;
            }
            catch (System.Exception ex)
            {
                if (Verbose)
                    Log?.LogDebug($"{Prefix} Failed to enter third person from wheel tilt: {ex.Message}");
            }
        }

        private void SmoothShoulder()
        {
            // --- Smooth LERP of our own shoulder blend ---
            ShoulderState.CurrentShoulderSign = Mathf.Lerp(
                ShoulderState.CurrentShoulderSign,
                ShoulderState.TargetShoulderSign,
                Time.deltaTime * _lerpSpeed
            );

            if (Mathf.Abs(ShoulderState.CurrentShoulderSign - ShoulderState.TargetShoulderSign) < 0.001f)
                ShoulderState.CurrentShoulderSign = ShoulderState.TargetShoulderSign;
        }

        private void CheckSceneChanged(float t)
        {
            if (t < _nextSceneCheck)
                return;

            _nextSceneCheck = t + 1f;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.handle == _lastSceneHandle)
                return;

            _lastSceneHandle = scene.handle;
            ShoulderState.ResetRuntimeState();
            CinemachineBrainPatches.ClearCachedState();
            PlayerCameraControllerPatches.ClearCachedState();

            if (Verbose)
                Log?.LogDebug($"{Prefix} Cleared camera caches after scene change: {scene.name}");
        }
    }
}
