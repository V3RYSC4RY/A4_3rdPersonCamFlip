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
        public static bool  InThirdPerson    = false;
        public static float CurrentOffset    = 0f;    // what is currently applied
        public static float TargetOffset     = 0f;    // what we are lerping toward
        public static bool  Enabled          = true;  // future toggle if you want keybind to disable
    }

    public class CameraShoulderController : MonoBehaviour
    {
        private static ManualLogSource Log => ThirdPersonCamFixPlugin.LogSource;
        private const string Prefix = "[TPCF] (CTRL)";

        // Local state
        private bool  _lastIsFirstPerson = true;

        // Interpret this as "are we on the default/right side?"
        // true  = default/right (no extra offset)
        // false = left (apply -_baseOffset)
        private bool  _useRightShoulder  = true;

        // These are now just *our* tuning knobs
        private float _baseOffset        = 1f;   // how far over the shoulder we shift WHEN ON LEFT
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
                Log?.LogMessage($"{Prefix} Update heartbeat – Time.time={t}");
            }

            if (!ShoulderState.Enabled)
                return;

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
                _useRightShoulder        = true;
                ShoulderState.TargetOffset  = 0f;

                ShoulderState.CurrentOffset = Mathf.Lerp(
                    ShoulderState.CurrentOffset,
                    ShoulderState.TargetOffset,
                    Time.deltaTime * _lerpSpeed
                );
                return;
            }

            // --- Shoulder toggle on C ---
            if (Input.GetKeyDown(KeyCode.C))
            {
                // Flip between default/right (0) and left (-baseOffset)
                _useRightShoulder = !_useRightShoulder;

                if (_useRightShoulder)
                {
                    // Default/right: DO NOT add extra offset
                    ShoulderState.TargetOffset = 0f;
                }
                else
                {
                    // Left: shift by -baseOffset (this is the one that "looks correct" for you)
                    ShoulderState.TargetOffset = -Mathf.Abs(_baseOffset);
                }

                Log?.LogMessage(
                    $"{Prefix} Shoulder toggled: {(_useRightShoulder ? "RIGHT (DEFAULT)" : "LEFT")} " +
                    $"TargetOffset={ShoulderState.TargetOffset}"
                );
            }

            // --- Smooth LERP of our own offset ---
            float before = ShoulderState.CurrentOffset;

            ShoulderState.CurrentOffset = Mathf.Lerp(
                ShoulderState.CurrentOffset,
                ShoulderState.TargetOffset,
                Time.deltaTime * _lerpSpeed
            );

            if (Mathf.Abs(ShoulderState.CurrentOffset - before) > 0.001f)
            {
                Log?.LogMessage(
                    $"{Prefix} Lerp offset: before={before}, target={ShoulderState.TargetOffset}, " +
                    $"after={ShoulderState.CurrentOffset}"
                );
            }
        }
    }
}
