using BepInEx.Logging;
using UnityEngine;

namespace ThirdPersonCamFix
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

        // +1 = right/default, -1 = left. We lerp between them for smoothing.
        public static float CurrentShoulderSign    = 1f;
        public static float TargetShoulderSign     = 1f;

        // Used only if the live camera has no baked-in lateral offset to mirror.
        public static float FallbackShoulderOffset = 1f;
        public static float FallbackTrackedOffset  = 0.6f; // for CameraManager path when the game reports 0
    }

    public class CameraShoulderController : MonoBehaviour
    {
        private static ManualLogSource Log => ThirdPersonCamFixPlugin.LogSource;
        private const string Prefix = "[TPCF] (CTRL)";

        // Local state
        private bool  _lastIsFirstPerson = true;

        // Interpret this as "are we on the default/right side?"
        // true  = default/right (no extra offset)
        // false = left (mirror the offset)
        private bool  _useRightShoulder  = true;

        // These are just *our* tuning knobs / fallbacks
        private float _baseOffset        = 1f;   // used only if the camera provides no baked-in offset
        private float _lerpSpeed         = 8f;   // how fast to blend
        private float _nextHeartbeat;

        private void Awake()
        {
            Log?.LogMessage($"{Prefix} Awake on GameObject: {gameObject.name}");
        }

        private void OnEnable()
        {
            Log?.LogMessage($"{Prefix} OnEnable; enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");
        }

        private void OnDisable()
        {
            Log?.LogMessage($"{Prefix} OnDisable; enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");
        }

        private void OnDestroy()
        {
            Log?.LogMessage($"{Prefix} OnDestroy on GameObject: {gameObject.name}");
        }

        private void Update()
        {
            float t = Time.time;

            // --- Heartbeat every ~1.5s so we know Update is alive ---
            if (t >= _nextHeartbeat)
            {
                _nextHeartbeat = t + 1.5f;
                Log?.LogMessage($"{Prefix} Update heartbeat - Time.time={t}");
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
                Log?.LogMessage($"{Prefix} View mode changed: {(isFirst ? "FIRST PERSON" : "THIRD PERSON")}");
            }

            ShoulderState.InThirdPerson = !isFirst;

            // Only mess with shoulder in third-person
            if (isFirst)
            {
                // When in first person, always reset to "default/right" so we don't carry weird offsets back
                _useRightShoulder           = true;
                ShoulderState.TargetShoulderSign = 1f;

                ShoulderState.CurrentShoulderSign = Mathf.Lerp(
                    ShoulderState.CurrentShoulderSign,
                    ShoulderState.TargetShoulderSign,
                    Time.deltaTime * _lerpSpeed
                );
                return;
            }

            // --- Shoulder toggle on C ---
            if (Input.GetKeyDown(KeyCode.C))
            {
                // Flip between default/right and left
                _useRightShoulder = !_useRightShoulder;

                ShoulderState.TargetShoulderSign = _useRightShoulder ? 1f : -1f;

                Log?.LogMessage(
                    $"{Prefix} Shoulder toggled: {(_useRightShoulder ? "RIGHT (DEFAULT)" : "LEFT")} " +
                    $"TargetSign={ShoulderState.TargetShoulderSign}"
                );
            }

            // --- Smooth LERP of our own shoulder blend ---
            ShoulderState.CurrentShoulderSign = Mathf.Lerp(
                ShoulderState.CurrentShoulderSign,
                ShoulderState.TargetShoulderSign,
                Time.deltaTime * _lerpSpeed
            );
        }
    }
}
